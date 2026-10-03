<template>
  <div class="relative flex min-h-0 flex-1 flex-col">
    <!-- Score panel: full-bleed by default — no margin, padding or border, so
         the score reaches every screen edge. The chrome bars overlay it
         instead of taking a slice of it, and only the roomy desktop layout
         floats it in a dashed frame (`desktop:` adds what mobile omits).

         `isolate` fixes the cursor overlap: alphaTab renders its playback
         cursor and bar highlight in a `.at-cursors` layer with z-index 1000,
         which would otherwise paint straight over the chrome bars (z-30/z-40)
         as they float across the score. A stacking context of its own keeps
         those z-indexes contained, so the bars always cover the cursor — and
         their glass blurs it instead. -->
    <div
      class="relative isolate flex min-h-0 flex-1 flex-col overflow-hidden bg-tartan desktop:m-4 desktop:rounded-xl desktop:border-4 desktop:border-dashed desktop:border-gold/70"
    >
      <!-- Placeholder when the song has no score file configured -->
      <div v-if="!song.filePath" class="flex flex-1 flex-col items-center justify-center gap-2 p-6 text-center">
        <p class="text-gold">Score viewport — notes &amp; tabs render here</p>
        <p class="text-xs text-cream/60">This song has no score file yet</p>
      </div>

      <template v-else>
        <!-- The scroller wraps alphaTab's container instead of being it: alphaTab
             measures its container's offsetWidth (border box), which a flex-stretched
             scroll container never shrinks when its own scrollbar appears — leaving a
             permanent ~15px horizontal overflow. As an auto-width block inside the
             scroller, the container's offsetWidth tracks the available width and
             alphaTab re-renders to fit; the reserved gutter (stable) prevents even a
             transient overflow while the vertical scrollbar settles.
             Cream "paper": alphaTab draws notation on a transparent surface. -->
        <div
          ref="scroller"
          class="score-scrollbar min-h-0 flex-1 overflow-y-auto bg-cream [scrollbar-gutter:stable]"
        >
          <!-- alphaTab renders into this element imperatively; Vue must not manage its children. -->
          <div ref="host" />
        </div>

        <!-- Continues the score's cream "paper" below the scroller. Hidden on
             mobile: there the paper must run edge to edge, this line is
             metadata rather than score (and the bottom chrome would cover it). -->
        <p
          v-if="phase === 'ready'"
          class="bg-cream px-3 pb-2 pt-1 text-center font-mono text-[10px] text-forest/60 mobile:hidden"
        >
          {{ song.filePath }}
        </p>

        <!-- Loading -->
        <div
          v-if="phase === 'loading'"
          class="absolute inset-0 z-10 flex items-center justify-center bg-tartan/90"
          role="status"
        >
          <span class="animate-pulse text-cream/60">Loading score…</span>
        </div>

        <!-- Error -->
        <div
          v-else-if="phase === 'error'"
          class="absolute inset-0 z-10 flex flex-col items-center justify-center gap-3 bg-tartan/95 p-6 text-center"
          role="alert"
        >
          <p class="text-lg text-clay">Couldn't load the score</p>
          <p class="text-sm text-cream/60">{{ errorMessage }}</p>
          <p class="font-mono text-xs text-cream/40">{{ song.filePath }}</p>
          <button
            type="button"
            class="rounded-full bg-gold px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream"
            @click="init"
          >
            Retry
          </button>
        </div>
      </template>
    </div>

    <!-- Bottom chrome: the track controls, the transport row and the song
         identity — this component owns all three.
         Mobile: one stack overlaying the score's bottom edge, slid away as a
         unit with a transform (the score underneath never resizes) and padded
         clear of the home indicator by .safe-area-bottom. Swiping anywhere on
         it — the footer or the open tray — up opens it onto the track
         controls, down closes it again.
         Desktop: a plain block — the tray inside positions itself over the
         score's top-left corner and the footer keeps its in-flow row, so the
         overflow clip that rounds the card is mobile-only. -->
    <div
      data-chrome
      class="liquid-glass mobile:liquid-frost rounded-xl safe-area-bottom transition-transform duration-300 ease-out mobile:absolute mobile:inset-x-3 mobile:bottom-3 mobile:z-30 mobile:flex mobile:flex-col mobile:overflow-hidden desktop:m-3"
      :class="{ 'mobile:translate-y-full': !chrome.visible }"
      :inert="!chrome.visible"
      @touchstart.passive="onSwipeStart"
      @touchend="onSwipeEnd"
    >
      <!-- Track selector: absolute column over the score's top-left corner
           on desktop, in-flow tray above the transport row on mobile.
           The wrapper is what the footer's expansion grows (0 -> the tray's
           content height), so the footer stays glued to the bottom edge and
           the score underneath never resizes; it is inert while closed so
           the hidden buttons stay out of the tab order. -->
      <div
        id="track-tray"
        ref="tray"
        class="max-h-[var(--tray-h)] overflow-x-hidden overflow-y-auto transition-[max-height] duration-300 ease-out desktop:max-h-none desktop:overflow-visible"
        :class="{ 'border-b border-cream/10': chrome.expanded }"
        :style="{ '--tray-h': `${trayHeight}px` }"
        :inert="!isDesktop && !chrome.expanded"
      >
        <ScoreTrackControls
          :tracks="trackControls"
          @mute="onToggleMute"
          @solo="onToggleSolo"
          @volume="onSetVolume"
          @open="onToggleRender"
        />
      </div>

      <!-- Three columns so the swipe indicator sits dead centre without ever
           colliding with the transport buttons or the (truncated) title. Each
           child claims its own column: on desktop the indicator is display
           none, and auto-placement would otherwise slide the title into the
           empty middle track instead of the right-hand one. (The swipe itself
           is bound on the whole bottom chrome, above; the hairline above this
           row belongs to the tray, and only shows when it is open.) -->
      <footer class="grid grid-cols-[1fr_auto_1fr] items-center gap-4 px-6 py-3">
        <PlayerControls class="col-start-1" :state="playbackState" @stop="stop" @play="play" @pause="pause" />

        <!-- Swipe indicator: up opens the track controls, down closes them.
             Also a plain button, so the gesture is not the only way in —
             keyboard and mouse users get one too. -->
        <button
          type="button"
          class="col-start-2 hidden rounded-full p-2 text-gold/70 transition hover:text-gold active:opacity-70 mobile:block"
          aria-controls="track-tray"
          :aria-expanded="chrome.expanded"
          :aria-label="chrome.expanded ? 'Hide track controls' : 'Show track controls'"
          :title="chrome.expanded ? 'Swipe down to hide the track controls' : 'Swipe up to show the track controls'"
          @click="toggleTrackTray"
        >
          <svg
            class="h-4 w-4 transition-transform duration-300"
            :class="chrome.expanded ? 'rotate-180' : ''"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            stroke-width="1.8"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
          >
            <path d="M4 10l4-4 4 4" />
          </svg>
        </button>

        <div class="col-start-3 min-w-0 text-right">
          <p class="truncate text-sm font-medium text-gold">{{ song.title }}</p>
          <p class="truncate text-xs text-cream/70">{{ song.author }}</p>
        </div>
      </footer>
    </div>
  </div>
</template>

<script setup lang="ts">
import { AlphaTabApi, FileLoadError, PlayerMode, synth } from '@coderline/alphatab'
import type { model } from '@coderline/alphatab'
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import type { PlaybackState } from '../types/playback'
import type { Song } from '../types/song'
import type { TrackControl, TrackKind } from '../types/track'
import { useDesktopViewport } from '../composables/useDesktopViewport'
import { useChromeStore } from '../stores/chrome'
import PlayerControls from './PlayerControls.vue'
import ScoreTrackControls from './ScoreTrackControls.vue'

const props = defineProps<{
  /** The song to render — its id fetches the score file, the rest identify it in the footer. */
  song: Song
}>()

/** Overlay visibility of the bottom chrome (and the header it rides with). */
const chrome = useChromeStore()

/** Expanding the footer onto the track controls is a mobile gesture. */
const isDesktop = useDesktopViewport()

type Phase = 'loading' | 'ready' | 'error'

const phase = ref<Phase>('loading')
const errorMessage = ref('')

const host = ref<HTMLDivElement | null>(null)
/** Scroll container around the alphaTab host — also alphaTab's cursor scroll element. */
const scroller = ref<HTMLDivElement | null>(null)

let api: AlphaTabApi | null = null
/** Event unsubscriptions returned by alphaTab's `.on(...)` */
let unsubscribers: Array<() => void> = []

/** Current transport state, mirrored onto the footer controls. */
const playbackState = ref<PlaybackState>('stopped')

/**
 * Per-track control state mirrored onto ScoreTrackControls.
 *
 * alphaTab keeps mute/solo/volume on the synthesizer's channels (not on the
 * model) and its Track objects are not reactive, so this array is the source
 * of truth the UI renders; every intent is pushed back into the API.
 */
const trackControls = ref<TrackControl[]>([])

function setPlaybackState(state: PlaybackState): void {
  if (state === playbackState.value) {
    return
  }
  playbackState.value = state
}

function teardown(): void {
  for (const off of unsubscribers) {
    off()
  }
  unsubscribers = []
  api?.destroy()
  api = null
}

function describeError(e: Error): string {
  if (e instanceof FileLoadError) {
    return e.xhr.status === 404
      ? 'The score file is missing on the server.'
      : `The score file could not be downloaded (HTTP ${e.xhr.status}).`
  }
  return e.message
}

/** Builds the reactive mirror of one score track's control state. */
function toTrackControl(track: model.Track): TrackControl {
  // File mute/solo flags are deliberately not consulted: a track that ships
  // with volume 0 is the only "starts muted" case, and its slider sitting at
  // 0 explains the silence just as well as a flag would.
  const volume = track.playbackInfo.volume / 16
  return {
    index: track.index,
    name: track.shortName || track.name || `Track ${track.index + 1}`,
    kind: detectTrackKind(track),
    volume,
    muted: volume === 0,
    solo: false,
    rendered: true,
  }
}

/** Icon family for a track: percussion flag first, then label, then GM program. */
function detectTrackKind(track: model.Track): TrackKind {
  if (track.isPercussion) {
    return 'drums'
  }
  const label = `${track.name} ${track.shortName}`.toLowerCase()
  if (/\bperc|drum/.test(label)) return 'drums'
  if (/bass/.test(label)) return 'bass'
  if (/gtr|guit/.test(label)) return 'guitar'
  if (/viol|cello|viola/.test(label)) return 'violin'
  if (/piano|keys|keyboard|organ/.test(label)) return 'piano'
  const program = track.playbackInfo.program
  if (program >= 32 && program <= 39) return 'bass'
  if (program >= 24 && program <= 31) return 'guitar'
  if (program >= 40 && program <= 47) return 'violin'
  if (program <= 7) return 'piano'
  return 'other'
}

/** The mirror for a track index, if the score exposed one. */
function trackControl(index: number): TrackControl | undefined {
  return trackControls.value.find((control) => control.index === index)
}

/** The alphaTab model track behind a mirror — score.tracks survives re-renders. */
function scoreTrack(index: number): model.Track | undefined {
  return api?.score?.tracks.find((track) => track.index === index)
}

/**
 * Pushes the mirrored track state into the synthesizer.
 *
 * alphaTab applies the file's channel volumes itself when the player becomes
 * ready — a handler registered in its constructor, before ours — so any
 * slider move made during soundfont loading would otherwise be clobbered
 * back to the file's value. Mute/solo are idempotent here.
 */
function applyTrackState(): void {
  for (const control of trackControls.value) {
    const track = scoreTrack(control.index)
    if (!track) {
      continue
    }
    api?.changeTrackMute([track], control.muted)
    api?.changeTrackSolo([track], control.solo)
    api?.changeTrackVolume([track], control.volume)
  }
}

/** (Re)creates the alphaTab instance and loads the song's score file. */
function init(): void {
  teardown()
  errorMessage.value = ''
  phase.value = 'loading'
  trackControls.value = []
  setPlaybackState('stopped')

  const element = host.value
  const scrollElement = scroller.value
  if (!element || !scrollElement || !props.song.filePath) {
    return
  }

  // Fresh element content for the new instance, view back at the top.
  element.replaceChildren()
  scrollElement.scrollTop = 0

  const alphaTabApi = new AlphaTabApi(element, {
    core: {
      // Absolute so fonts resolve under SPA routes like /songs/1 as well;
      // the Vite plugin serves them from <app-root>/font.
      fontDirectory: '/font/',
    },
    player: {
      // Synthesizer playback with the SONiVOX soundfont the plugin copies to
      // <app-root>/soundfont; loaded automatically once the player is ready.
      playerMode: PlayerMode.EnabledSynthesizer,
      soundFont: '/soundfont/sonivox.sf2',
      enableCursor: true,
      // The document itself never scrolls (h-dvh layout), so cursor
      // auto-scrolling must target the score panel's own scroller.
      scrollElement,
    },
  })

  unsubscribers.push(
    alphaTabApi.scoreLoaded.on((score) => {
      phase.value = 'ready'
      trackControls.value = score.tracks.map(toTrackControl)
    }),
    alphaTabApi.error.on((e) => {
      errorMessage.value = describeError(e)
      phase.value = 'error'
      trackControls.value = []
    }),
    alphaTabApi.playerReady.on(() => applyTrackState()),
    alphaTabApi.playerStateChanged.on((args) => {
      if (args.stopped) {
        setPlaybackState('stopped')
      } else {
        setPlaybackState(args.state === synth.PlayerState.Playing ? 'playing' : 'paused')
      }
    }),
    alphaTabApi.playerFinished.on(() => setPlaybackState('stopped')),
  )

  api = alphaTabApi
  alphaTabApi.load(`/api/songs/${props.song.id}/file`)
}

/** Transport controls, wired to the footer's PlayerControls intents. */
function play(): void {
  if (phase.value === 'ready' && playbackState.value !== 'playing') {
    api?.play()
  }
}

function pause(): void {
  if (playbackState.value === 'playing') {
    api?.pause()
  }
}

function stop(): void {
  api?.stop()
  // Stop means back to the beginning — reset the panel view with the cursor.
  if (scroller.value) {
    scroller.value.scrollTop = 0
  }
}

/** Track control intents, wired to ScoreTrackControls' emits. */
function onToggleMute(index: number): void {
  const control = trackControl(index)
  const track = scoreTrack(index)
  if (!control || !track) {
    return
  }
  control.muted = !control.muted
  api?.changeTrackMute([track], control.muted)
}

function onToggleSolo(index: number): void {
  const control = trackControl(index)
  const track = scoreTrack(index)
  if (!control || !track) {
    return
  }
  control.solo = !control.solo
  api?.changeTrackSolo([track], control.solo)
}

function onSetVolume(index: number, volume: number): void {
  const control = trackControl(index)
  const track = scoreTrack(index)
  if (!control || !track) {
    return
  }
  control.volume = volume
  api?.changeTrackVolume([track], volume)
}

/** The open button isolates a track; pressing it again while isolated
 *  brings the full score back. */
function onToggleRender(index: number): void {
  const score = api?.score
  if (!api || !score) {
    return
  }
  const isolated = api.tracks.length === 1 && api.tracks[0].index === index
  const next = isolated
    ? score.tracks
    : score.tracks.filter((track) => track.index === index)
  api.renderTracks(next)
  for (const control of trackControls.value) {
    control.rendered = next.some((track) => track.index === control.index)
  }
}

/**
 * Expanding footer (mobile)
 *
 * The track controls sit in a wrapper between the score and the footer that
 * grows from 0 to their content height when the footer is swiped open, so the
 * footer stays pinned to the bottom edge and the score itself never resizes.
 */

/** Wrapper around ScoreTrackControls that the expansion grows. */
const tray = ref<HTMLElement | null>(null)
/** Its animated height: 0 while closed, the tray's content height while open. */
const trayHeight = ref(0)
/** Vertical travel (px), and the horizontal slack it must beat, to count as a swipe. */
const FOOTER_SWIPE_PX = 40
/** Headroom above the open tray: the header plus a readable strip of score. */
const TRAY_TOP_RESERVE = 160
/** Even a tiny viewport keeps the tray tall enough for a couple of rows. */
const TRAY_MIN_HEIGHT = 120
let swipeStartY: number | null = null
let swipeStartX = 0

/** Points the height transition at what the tray actually holds. */
function syncTrayHeight(): void {
  if (!chrome.expanded || !tray.value) {
    trayHeight.value = 0
    return
  }
  // scrollHeight reports the full content even while the wrapper is clipped
  // to 0, so this is exactly what the transition animates towards. The cap
  // only bites when a long track list would otherwise swallow the whole
  // score — the wrapper scrolls then, so every control stays reachable.
  const room = Math.max(TRAY_MIN_HEIGHT, window.innerHeight - TRAY_TOP_RESERVE)
  trayHeight.value = Math.min(tray.value.scrollHeight, room)
}

/** The arrow in the middle of the footer. */
function toggleTrackTray(): void {
  if (chrome.expanded) {
    chrome.collapse()
  } else {
    chrome.expand()
  }
}

function onSwipeStart(event: TouchEvent): void {
  const touch = event.touches[0]
  swipeStartY = touch ? touch.clientY : null
  swipeStartX = touch ? touch.clientX : 0
}

/** Swipe up on the bottom chrome (footer or the open tray) opens the track
 *  controls, down closes them. */
function onSwipeEnd(event: TouchEvent): void {
  const startY = swipeStartY
  swipeStartY = null
  const touch = event.changedTouches[0]
  if (startY === null || !touch || isDesktop.value) return
  const deltaY = touch.clientY - startY
  const deltaX = touch.clientX - swipeStartX
  // Vertical travel only: a sideways drag over the controls is not a swipe.
  if (Math.abs(deltaY) < FOOTER_SWIPE_PX || Math.abs(deltaY) < Math.abs(deltaX)) return
  // Consumed, so the browser doesn't turn the release into a click on
  // whichever control the finger happened to end over.
  event.preventDefault()
  if (deltaY < 0) {
    chrome.expand()
  } else {
    chrome.collapse()
  }
}

// The wrapper follows what it holds: the expansion itself, and the track list
// appearing or changing underneath it while it is open. Runs before the DOM
// update, so the measurement still sees the collapsed height.
watch([() => chrome.expanded, () => trackControls.value.length], syncTrayHeight)

onMounted(() => {
  // The cap depends on the viewport, so a resize re-measures an open tray.
  window.addEventListener('resize', syncTrayHeight)
  init()
})
onBeforeUnmount(() => {
  window.removeEventListener('resize', syncTrayHeight)
  teardown()
})
watch(
  () => props.song.id,
  () => init(),
)
</script>
