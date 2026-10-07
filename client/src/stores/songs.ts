import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSongs } from '../services/songApi'
import type { Song } from '../types/song'

/**
 * The song list for the songs list page.
 *
 * Songs are shared: `GET /songs` returns the whole catalogue for everyone,
 * so there is no ownership filter at this stage.
 *
 * UI states: loading, ready (songs set), error.
 */
export const useSongsStore = defineStore('songs', () => {
  const songs = ref<Song[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Guards against a stale response when a reload overlaps an earlier one.
  let requestSeq = 0

  async function load(): Promise<void> {
    const requestId = ++requestSeq
    loading.value = true
    error.value = null

    try {
      const result = await fetchSongs()
      if (requestId !== requestSeq) return
      songs.value = result
    } catch (e) {
      if (requestId !== requestSeq) return
      songs.value = []
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      if (requestId === requestSeq) {
        loading.value = false
      }
    }
  }

  return { songs, loading, error, load }
})
