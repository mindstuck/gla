<template>
  <div
    class="relative m-4 flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border-2 border-dashed border-gold/30 bg-tartan"
  >
    <!-- Placeholder when the song has no score file configured -->
    <div v-if="!filePath" class="flex flex-1 flex-col items-center justify-center gap-2 p-6 text-center">
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
        class="min-h-0 flex-1 overflow-y-auto bg-cream [scrollbar-gutter:stable]"
      >
        <!-- alphaTab renders into this element imperatively; Vue must not manage its children. -->
        <div ref="host" />
      </div>

      <p v-if="phase === 'ready'" class="px-3 pb-2 pt-1 text-center font-mono text-[10px] text-cream/40">
        {{ filePath }}
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
        <p class="font-mono text-xs text-cream/40">{{ filePath }}</p>
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
</template>

<script setup lang="ts">
import { AlphaTabApi, FileLoadError, PlayerMode, synth } from '@coderline/alphatab'
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import type { PlaybackState } from '../types/playback'

const props = defineProps<{
  /** Backend song id — the score file is fetched from /api/songs/{id}/file */
  songId: number
  /** Song file location (relative to the files root), shown while the score loads */
  filePath?: string
}>()

const emit = defineEmits<{
  /** Current playback state, mirrored up so the footer controls can reflect it. */
  playerState: [state: PlaybackState]
}>()

type Phase = 'loading' | 'ready' | 'error'

const phase = ref<Phase>('loading')
const errorMessage = ref('')

const host = ref<HTMLDivElement | null>(null)
/** Scroll container around the alphaTab host — also alphaTab's cursor scroll element. */
const scroller = ref<HTMLDivElement | null>(null)

let api: AlphaTabApi | null = null
/** Event unsubscriptions returned by alphaTab's `.on(...)` */
let unsubscribers: Array<() => void> = []

/** Last state pushed to the parent — events are emitted only on change. */
let playbackState: PlaybackState = 'stopped'

function setPlaybackState(state: PlaybackState): void {
  if (state === playbackState) {
    return
  }
  playbackState = state
  emit('playerState', state)
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

/** (Re)creates the alphaTab instance and loads the song's score file. */
function init(): void {
  teardown()
  errorMessage.value = ''
  phase.value = 'loading'
  setPlaybackState('stopped')

  const element = host.value
  const scrollElement = scroller.value
  if (!element || !scrollElement || !props.filePath) {
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
    alphaTabApi.scoreLoaded.on(() => {
      phase.value = 'ready'
    }),
    alphaTabApi.error.on((e) => {
      errorMessage.value = describeError(e)
      phase.value = 'error'
    }),
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
  alphaTabApi.load(`/api/songs/${props.songId}/file`)
}

/** Controls exposed to SongView, which owns the footer buttons. */
function play(): void {
  if (phase.value === 'ready' && playbackState !== 'playing') {
    api?.play()
  }
}

function pause(): void {
  if (playbackState === 'playing') {
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

defineExpose({ play, pause, stop })

onMounted(init)
onBeforeUnmount(teardown)
watch(
  () => props.songId,
  () => init(),
)
</script>
