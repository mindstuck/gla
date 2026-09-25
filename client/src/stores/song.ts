import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSongById } from '../services/songApi'
import type { Song } from '../types/song'

/**
 * Current song state for the song page.
 *
 * UI states: loading, ready (song set), notFound, error.
 */
export const useSongStore = defineStore('song', () => {
  const song = ref<Song | null>(null)
  const loading = ref(false)
  const notFound = ref(false)
  const error = ref<string | null>(null)

  // Guards against stale responses when the id changes mid-flight.
  let requestSeq = 0

  async function load(id: number): Promise<void> {
    const requestId = ++requestSeq
    loading.value = true
    notFound.value = false
    error.value = null

    try {
      const result = await fetchSongById(id)
      if (requestId !== requestSeq) return
      song.value = result
      notFound.value = result === null
    } catch (e) {
      if (requestId !== requestSeq) return
      song.value = null
      error.value = e instanceof Error ? e.message : 'Unknown error'
    } finally {
      if (requestId === requestSeq) {
        loading.value = false
      }
    }
  }

  function reset(): void {
    requestSeq += 1
    song.value = null
    loading.value = false
    notFound.value = false
    error.value = null
  }

  return { song, loading, notFound, error, load, reset }
})
