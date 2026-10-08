import { defineStore } from 'pinia'
import { ref } from 'vue'
import { deleteSong, fetchSongs } from '../services/songApi'
import type { Song } from '../types/song'

/**
 * The song list for the songs list page.
 *
 * Songs are shared: `GET /songs` returns the whole catalogue for everyone,
 * so there is no ownership filter at this stage.
 *
 * UI states: loading, ready (songs set), error, plus deleteError — a failed
 * delete must not take over the page, it only reports itself above the list.
 */
export const useSongsStore = defineStore('songs', () => {
  const songs = ref<Song[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const deleteError = ref<string | null>(null)

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

  /**
   * Deletes a song, then drops it from the list. The row only disappears
   * once the server confirmed (204); failures keep it and report inline.
   */
  async function remove(id: number): Promise<void> {
    deleteError.value = null
    try {
      await deleteSong(id)
      songs.value = songs.value.filter((song) => song.id !== id)
    } catch (e) {
      deleteError.value = e instanceof Error ? e.message : 'Could not delete the song'
    }
  }

  return { songs, loading, error, deleteError, load, remove }
})
