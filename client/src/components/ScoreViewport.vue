<template>
  <div
    class="m-4 flex min-h-0 flex-1 flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed border-gold/30 bg-tartan p-6"
  >
    <p class="text-gold">Score viewport — notes &amp; tabs render here</p>
    <p class="text-xs text-cream/60">Rendering arrives with the alphaTab integration</p>
    <p v-if="filePath" class="font-mono text-xs text-cream/50">{{ filePath }}</p>
  </div>
</template>

<script setup lang="ts">
/**
 * Placeholder for the score viewport.
 *
 * alphaTab integration (post-Step 6) will replace the placeholder content:
 * this component will own the DOM element alphaTab mounts into, fetch the file
 * at `filePath`, parse it and render notes + tabs. Until then it only shows
 * where the score will appear and which file it will come from.
 */
defineProps<{
  /** Song file location, e.g. "/ParanoidAndroid.gp5" */
  filePath?: string
}>()
</script>
