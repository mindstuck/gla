<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import SongUploadDialog from '../components/SongUploadDialog.vue'
import { useSongsStore } from '../stores/songs'
import type { Song } from '../types/song'

const store = useSongsStore()
const router = useRouter()
const dialogOpen = ref(false)

onMounted(() => void store.load())

// Upload finished: refresh the catalogue behind us and jump straight to the song.
function onUploaded(song: Song): void {
  dialogOpen.value = false
  void store.load()
  void router.push({ name: 'song', params: { id: song.id } })
}
</script>

<template>
  <!-- The shell never scrolls (h-dvh, overflow-hidden), so the list owns its
       own scroller. The header floats over its top edge on mobile — hence the
       larger top padding there; on desktop the header sits above it in flow. -->
  <div
    class="score-scrollbar min-h-0 flex-1 overflow-y-auto px-6 pt-8 pb-12 mobile:pt-24"
  >
    <div class="mx-auto w-full max-w-2xl">
      <div class="mb-6 flex items-center gap-3">
        <h2 class="font-display text-xl tracking-wide text-gold">Songs</h2>
        <button
          type="button"
          class="flex h-9 w-9 items-center justify-center rounded-full bg-gold text-forest transition hover:bg-cream"
          aria-label="Add song"
          title="Add song"
          @click="dialogOpen = true"
        >
          <svg
            class="h-5 w-5"
            viewBox="0 0 20 20"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            aria-hidden="true"
          >
            <path d="M10 4.5v11M4.5 10h11" />
          </svg>
        </button>
      </div>

      <!-- Loading -->
      <p v-if="store.loading" class="animate-pulse text-cream/60" role="status">
        Loading songs…
      </p>

      <!-- Error -->
      <div v-else-if="store.error" class="flex flex-col items-start gap-3">
        <p class="text-gold">Couldn't load the songs</p>
        <p class="text-sm text-cream/60">{{ store.error }}</p>
        <button
          type="button"
          class="rounded-full bg-gold px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream"
          @click="store.load()"
        >
          Retry
        </button>
      </div>

      <!-- Empty -->
      <p v-else-if="store.songs.length === 0" class="text-cream/60">
        No songs yet.
      </p>

      <!-- Ready — the whole shared catalogue, one row per song. -->
      <ul v-else class="space-y-1">
        <li v-for="song in store.songs" :key="song.id">
          <router-link
            :to="{ name: 'song', params: { id: song.id } }"
            class="group flex items-baseline justify-between gap-4 rounded-xl px-4 py-3.5 transition hover:bg-forest"
          >
            <span class="truncate text-gold group-hover:text-cream">
              {{ song.title }}
            </span>
            <span class="shrink-0 text-sm text-cream/60 group-hover:text-cream/80">
              {{ song.author }}
            </span>
          </router-link>
        </li>
      </ul>
    </div>
  </div>

  <SongUploadDialog
    :open="dialogOpen"
    @close="dialogOpen = false"
    @uploaded="onUploaded"
  />
</template>
