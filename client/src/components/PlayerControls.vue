<script setup lang="ts">
import type { PlaybackState } from '../types/playback'

/**
 * Playback controls for the song page footer.
 *
 * Dumb presentational component: it only emits intents and mirrors the
 * player's current state onto the buttons (gold fill on the active one).
 * Play and Pause share a single toggle button — icon, label and emitted
 * intent follow the current state. Binding happens in SongView via
 * ScoreViewport's exposed controls.
 */
const props = defineProps<{
  state: PlaybackState
}>()

const emit = defineEmits<{
  stop: []
  play: []
  pause: []
}>()

/** Gold fill marks the button matching the active player state. */
function stateClass(active: boolean): string {
  return active
    ? 'bg-gold text-forest hover:bg-cream'
    : 'bg-tartan text-cream hover:bg-forest hover:text-gold'
}

/** One button for play/pause: while playing it pauses, otherwise it plays. */
function onToggle(): void {
  if (props.state === 'playing') {
    emit('pause')
  } else {
    emit('play')
  }
}
</script>

<template>
  <div class="flex items-center gap-2" role="group" aria-label="Playback controls">
    <button
      type="button"
      aria-label="Stop"
      title="Stop"
      :aria-pressed="state === 'stopped'"
      :class="['rounded-full p-2 transition', stateClass(state === 'stopped')]"
      @click="$emit('stop')"
    >
      <svg class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
        <rect x="3" y="3" width="10" height="10" rx="1" />
      </svg>
    </button>

    <button
      type="button"
      :aria-label="state === 'playing' ? 'Pause' : 'Play'"
      :title="state === 'playing' ? 'Pause' : 'Play'"
      :aria-pressed="state === 'playing'"
      :class="['rounded-full p-2 transition', stateClass(state === 'playing')]"
      @click="onToggle"
    >
      <svg
        v-if="state === 'playing'"
        class="h-4 w-4"
        viewBox="0 0 16 16"
        fill="currentColor"
        aria-hidden="true"
      >
        <rect x="3.5" y="3" width="3" height="10" rx="1" />
        <rect x="9.5" y="3" width="3" height="10" rx="1" />
      </svg>
      <svg v-else class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
        <path d="M4 2.5v11l9-5.5-9-5.5z" />
      </svg>
    </button>
  </div>
</template>
