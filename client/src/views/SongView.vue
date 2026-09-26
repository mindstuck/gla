<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import PlayerControls from '../components/PlayerControls.vue'
import ScoreViewport from '../components/ScoreViewport.vue'
import { useSongStore } from '../stores/song'

const route = useRoute()
const store = useSongStore()

const songId = computed(() => Number(route.params.id))

function load(): void {
  const id = songId.value
  if (Number.isInteger(id) && id > 0) {
    void store.load(id)
  }
}

onMounted(load)
watch(() => route.params.id, load)

// Intents only: binding to the alphaTab player happens in the post-Step 6
// integration, so these handlers are replaced without touching the template.
function onStop(): void {}
function onPlay(): void {}
function onPause(): void {}
</script>

<template>
  <div class="flex min-h-0 flex-1 flex-col">
    <!-- Loading -->
    <div
      v-if="store.loading"
      class="flex flex-1 items-center justify-center text-cream/60"
      role="status"
    >
      <span class="animate-pulse">Loading song…</span>
    </div>

    <!-- Not found (404 or malformed route id) -->
    <div v-else-if="store.notFound || !Number.isInteger(songId) || songId < 1" class="flex flex-1 flex-col items-center justify-center gap-3">
      <p class="text-lg text-gold">Song not found</p>
      <p class="text-sm text-cream/60">No song with id {{ route.params.id }}.</p>
      <router-link
        to="/"
        class="rounded-full bg-gold px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream"
      >
        Back to songs
      </router-link>
    </div>

    <!-- Error -->
    <div v-else-if="store.error" class="flex flex-1 flex-col items-center justify-center gap-3">
      <p class="text-lg text-gold">Couldn't load the song</p>
      <p class="text-sm text-cream/60">{{ store.error }}</p>
      <button
        type="button"
        class="rounded-full bg-gold px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream"
        @click="load"
      >
        Retry
      </button>
    </div>

    <!-- Ready -->
    <template v-else-if="store.song">
      <ScoreViewport :song-id="store.song.id" :file-path="store.song.filePath" />

      <footer class="flex items-center justify-between gap-4 border-t border-cream/10 px-6 py-3">
        <PlayerControls @stop="onStop" @play="onPlay" @pause="onPause" />

        <div class="min-w-0 text-right">
          <p class="truncate text-sm font-medium text-gold">{{ store.song.title }}</p>
          <p class="truncate text-xs text-cream/70">{{ store.song.author }}</p>
        </div>
      </footer>
    </template>

    <!-- Initial state before first load resolves (nothing else to show) -->
    <div v-else class="flex flex-1 items-center justify-center text-cream/40">
      No song selected
    </div>
  </div>
</template>
