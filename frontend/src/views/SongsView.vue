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
       larger top padding there; on desktop the header sits above it in flow.
       The column fills the scroller (min-h-full), which lets the link row at
       its foot sit at the page's bottom (mt-auto) while the list is short. -->
  <div
    class="score-scrollbar min-h-0 flex-1 overflow-y-auto px-6 pt-8 pb-12 mobile:pt-24"
  >
    <div class="mx-auto flex min-h-full w-full max-w-2xl flex-col">
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

      <!-- A failed delete stays local: reported here, list untouched. -->
      <p
        v-if="store.deleteError"
        class="mb-3 text-sm text-clay"
        role="alert"
      >
        {{ store.deleteError }}
      </p>

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
        <li
          v-for="song in store.songs"
          :key="song.id"
          class="group relative flex items-center rounded-xl transition hover:bg-forest"
        >
          <router-link
            :to="{ name: 'song', params: { id: song.id } }"
            class="flex min-w-0 flex-1 items-baseline justify-between gap-4 px-4 py-3.5 transition-all mobile:pr-14 desktop:pr-4 desktop:group-hover:pr-14 desktop:group-focus-within:pr-14"
          >
            <span class="truncate text-gold group-hover:text-cream">
              {{ song.title }}
            </span>
            <span class="shrink-0 text-sm text-cream/60 group-hover:text-cream/80">
              {{ song.author }}
            </span>
          </router-link>

          <!-- Circle delete: always there on touch (no hover to reveal it),
               hidden on desktop until the row is hovered or focused — the
               link's right padding opens up at the same moment, so the icon
               never lands on the author text. -->
          <button
            type="button"
            class="absolute right-2 top-1/2 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-full border border-clay/50 text-clay transition hover:border-clay hover:bg-clay hover:text-forest focus-visible:outline-2 focus-visible:outline-gold mobile:opacity-100 desktop:opacity-0 desktop:group-hover:opacity-100 desktop:group-focus-within:opacity-100"
            :aria-label="`Delete ${song.title}`"
            :title="`Delete ${song.title}`"
            @click="store.remove(song.id)"
          >
            <svg
              class="h-4 w-4"
              viewBox="0 0 16 16"
              fill="none"
              stroke="currentColor"
              stroke-width="1.75"
              stroke-linecap="round"
              aria-hidden="true"
            >
              <path d="M4.75 4.75l6.5 6.5M11.25 4.75l-6.5 6.5" />
            </svg>
          </button>
        </li>
      </ul>

      <!-- The page's foot: one row of small, light links, one icon each.
           Where to find files to upload, and where the project lives. -->
      <div class="mt-auto flex items-center gap-5 pt-10 text-xs font-light text-cream/60">
        <a
          href="https://gprotab.net/"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-1.5 transition hover:text-gold"
        >
          <svg
            class="h-3.5 w-3.5"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
            aria-hidden="true"
          >
            <circle cx="6.75" cy="6.75" r="4.5" />
            <path d="M10.1 10.1 13.5 13.5" />
          </svg>
          Find songs to upload
        </a>
        <a
          href="https://github.com/mindstuck/gla"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-1.5 transition hover:text-gold"
        >
          <svg
            class="h-3.5 w-3.5"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
            aria-hidden="true"
          >
            <circle cx="6.75" cy="6.75" r="4.5" />
            <path d="M10.1 10.1 13.5 13.5" />
          </svg>
          github
        </a>
      </div>
    </div>
  </div>

  <SongUploadDialog
    :open="dialogOpen"
    @close="dialogOpen = false"
    @uploaded="onUploaded"
  />
</template>
