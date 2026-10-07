<script setup lang="ts">
import { onMounted } from 'vue'
import { useSongsStore } from '../stores/songs'

const store = useSongsStore()

onMounted(() => void store.load())
</script>

<template>
  <!-- The shell never scrolls (h-dvh, overflow-hidden), so the list owns its
       own scroller. The header floats over its top edge on mobile — hence the
       larger top padding there; on desktop the header sits above it in flow. -->
  <div
    class="score-scrollbar min-h-0 flex-1 overflow-y-auto px-6 pt-8 pb-12 mobile:pt-24"
  >
    <div class="mx-auto w-full max-w-2xl">
      <h2 class="mb-6 font-display text-xl tracking-wide text-gold">Songs</h2>

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
</template>
