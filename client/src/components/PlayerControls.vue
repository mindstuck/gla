<script setup lang="ts">
/**
 * Playback controls for the song page footer.
 *
 * Dumb presentational component: it only emits intents. Binding to the actual
 * player (alphaTab, post-Step 6) happens in the parent, so the buttons stay
 * untouched when the player API arrives.
 */
defineEmits<{
  stop: []
  play: []
  pause: []
}>()
</script>

<template>
  <div class="flex items-center gap-2">
    <button
      type="button"
      aria-label="Stop"
      class="rounded-md bg-tartan p-2 text-cream transition hover:bg-forest hover:text-gold"
      @click="$emit('stop')"
    >
      <svg class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
        <rect x="3" y="3" width="10" height="10" rx="1" />
      </svg>
    </button>

    <button
      type="button"
      aria-label="Play"
      class="rounded-md bg-tartan p-2 text-cream transition hover:bg-forest hover:text-gold"
      @click="$emit('play')"
    >
      <svg class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
        <path d="M4 2.5v11l9-5.5-9-5.5z" />
      </svg>
    </button>

    <button
      type="button"
      aria-label="Pause"
      class="rounded-md bg-tartan p-2 text-cream transition hover:bg-forest hover:text-gold"
      @click="$emit('pause')"
    >
      <svg class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
        <rect x="3.5" y="3" width="3" height="10" rx="1" />
        <rect x="9.5" y="3" width="3" height="10" rx="1" />
      </svg>
    </button>
  </div>
</template>
