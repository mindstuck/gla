<template>
  <div class="relative flex min-h-0 flex-1 flex-col">
    <!-- Score panel: full-bleed by default — no margin, padding or border, so
         the score reaches every screen edge. The chrome bars overlay it
         instead of taking a slice of it, and only the roomy desktop layout
         floats it in a dashed frame (`desktop:` adds what mobile omits). -->
    <div
      class="relative flex min-h-0 flex-1 flex-col overflow-hidden bg-tartan desktop:m-4 desktop:rounded-xl desktop:border-4 desktop:border-dashed desktop:border-gold/70"
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

    <!-- Bottom chrome: the track selector plus the transport controls and
         song identity — this component owns all three.
         Mobile: one stack overlaying the score's bottom edge, slid away as a
         unit with a transform (the score underneath never resizes) and padded
         clear of the home indicator by .safe-area-bottom.
         Desktop: a plain block — the tray inside positions itself over the
         score's top-left corner and the footer keeps its in-flow row. -->
    <div
      data-chrome
      class="safe-area-bottom transition-transform duration-300 ease-out mobile:absolute mobile:inset-x-0 mobile:bottom-0 mobile:z-30 mobile:flex mobile:flex-col mobile:bg-bottle"
      :class="{ 'mobile:translate-y-full': !chrome.visible }"
      :inert="!chrome.visible"
    >
      <!-- Track selector: absolute column over the score's top-left corner
           on desktop, in-flow tray above the transport row on mobile. -->
      <ScoreTrackControls
        :tracks="trackControls"
        @mute="onToggleMute"
        @solo="onToggleSolo"
        @volume="onSetVolume"
        @open="onToggleRender"
      />

      <footer class="flex items-center justify-between gap-4 border-t border-cream/10 px-6 py-3">
        <PlayerControls :state="playbackState" @stop="stop" @play="play" @pause="pause" />

        <div class="min-w-0 text-right">
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
import { useChromeStore } from '../stores/chrome'
import PlayerControls from './PlayerControls.vue'
import ScoreTrackControls from './ScoreTrackControls.vue'

const props = defineProps<{
  /** The song to render — its id fetches the score file, the rest identify it in the footer. */
  song: Song
}>()

/** Overlay visibility of the bottom chrome (and the header it rides with). */
const chrome = useChromeStore()

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

onMounted(init)
onBeforeUnmount(teardown)
watch(
  () => props.song.id,
  () => init(),
)
</script>
