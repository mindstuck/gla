<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue'
import { ApiError, uploadSong } from '../services/songApi'
import type { Song } from '../types/song'

/**
 * Modal with a single-file picker for adding a song. The accept list mirrors
 * the server's SongFileStore.SupportedExtensions (both spellings for alphaTex:
 * ".capalphaTex" as specified, ".alphatex" as alphaTab names it).
 */
const ACCEPT = '.gp3,.gp4,.gp5,.gpx,.gp,.musicxml,.xml,.capx,.capalphaTex,.alphatex'
const ACCEPTED = new Set(ACCEPT.split(',').map((ext) => ext.slice(1).toLowerCase()))

const props = defineProps<{ open: boolean }>()
const emit = defineEmits<{ close: []; uploaded: [song: Song] }>()

const file = ref<File | null>(null)
const title = ref('')
const author = ref('')
const uploading = ref(false)
const error = ref<string | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)

function reset(): void {
  file.value = null
  title.value = ''
  author.value = ''
  error.value = null
  uploading.value = false
}

function onKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape') emit('close')
}

// Every opening starts clean: no leftover file, no stale error, focus in the picker.
watch(
  () => props.open,
  (open) => {
    if (open) {
      reset()
      document.addEventListener('keydown', onKeydown)
      requestAnimationFrame(() => fileInput.value?.focus())
    } else {
      document.removeEventListener('keydown', onKeydown)
    }
  },
)

onBeforeUnmount(() => document.removeEventListener('keydown', onKeydown))

function extensionOf(name: string): string {
  const dot = name.lastIndexOf('.')
  return dot < 0 ? '' : name.slice(dot + 1).toLowerCase()
}

function onPick(event: Event): void {
  const input = event.target as HTMLInputElement
  const picked = input.files?.[0] ?? null
  error.value = null

  // The server checks again; failing here just saves a round trip.
  if (picked && !ACCEPTED.has(extensionOf(picked.name))) {
    file.value = null
    input.value = ''
    error.value = `“${picked.name}” is not a supported score file.`
    return
  }

  file.value = picked
}

async function submit(): Promise<void> {
  if (file.value === null || uploading.value) return

  uploading.value = true
  error.value = null
  try {
    // The parent reacts by refetching the list and opening the new song.
    emit(
      'uploaded',
      await uploadSong(
        file.value,
        title.value.trim() || undefined,
        author.value.trim() || undefined,
      ),
    )
  } catch (e) {
    error.value = e instanceof ApiError ? e.message : 'Upload failed'
  } finally {
    uploading.value = false
  }
}
</script>

<template>
  <Teleport to="body">
    <!-- Above the chrome cards (z-40); the backdrop closes the modal. -->
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4"
      @click.self="emit('close')"
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="add-song-title"
        class="w-full max-w-sm rounded-3xl border border-gold/15 bg-forest p-6 shadow-2xl"
      >
        <h3 id="add-song-title" class="font-display text-lg tracking-wide text-gold">
          Add song
        </h3>
        <p class="mt-1 text-sm text-cream/60">
          One score file at a time: Guitar Pro, MusicXML, Capella or alphaTex.
        </p>

        <label
          class="mt-4 flex cursor-pointer items-center gap-3 rounded-xl border border-gold/25 px-4 py-3 transition hover:border-gold/50 hover:bg-bottle/40"
        >
          <input
            ref="fileInput"
            type="file"
            class="sr-only"
            :accept="ACCEPT"
            @change="onPick"
          />
          <span
            class="shrink-0 rounded-full bg-gold px-3 py-1 text-xs font-medium text-forest"
          >
            Choose file
          </span>
          <span class="min-w-0 flex-1 truncate text-sm text-cream/70">
            {{ file ? file.name : 'No file chosen' }}
          </span>
        </label>

        <!-- Optional metadata: left blank, the server falls back to the
             file name for the title. -->
        <label class="sr-only" for="song-title">Title (optional)</label>
        <input
          id="song-title"
          v-model="title"
          type="text"
          maxlength="200"
          placeholder="Title (optional — falls back to the file name)"
          class="mt-3 w-full rounded-xl border border-gold/25 bg-bottle/40 px-4 py-2.5 text-sm text-cream outline-none transition placeholder:text-cream/40 focus:border-gold/50"
        />
        <label class="sr-only" for="song-author">Author (optional)</label>
        <input
          id="song-author"
          v-model="author"
          type="text"
          maxlength="200"
          placeholder="Author (optional)"
          class="mt-3 w-full rounded-xl border border-gold/25 bg-bottle/40 px-4 py-2.5 text-sm text-cream outline-none transition placeholder:text-cream/40 focus:border-gold/50"
        />

        <p v-if="error" class="mt-3 text-sm text-clay" role="alert">{{ error }}</p>

        <div class="mt-6 flex items-center justify-end gap-3">
          <button
            type="button"
            class="rounded-full px-4 py-1.5 text-sm text-cream/70 transition hover:text-gold"
            @click="emit('close')"
          >
            Cancel
          </button>
          <button
            type="button"
            class="rounded-full bg-gold px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream disabled:cursor-not-allowed disabled:opacity-50"
            :disabled="file === null || uploading"
            @click="submit"
          >
            {{ uploading ? 'Uploading…' : 'Upload' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
