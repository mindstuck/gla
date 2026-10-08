<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue'

/**
 * Yes/no confirmation in the same modal shell as the upload dialog. Focus
 * lands on Cancel, so a stray Enter never fires the confirm action.
 */
const props = defineProps<{
  open: boolean
  title: string
  message: string
  confirmLabel?: string
}>()
const emit = defineEmits<{ close: []; confirm: [] }>()

const cancel = ref<HTMLButtonElement | null>(null)

function onKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape') emit('close')
}

// Every opening arms Escape and focuses the safe button.
watch(
  () => props.open,
  (open) => {
    if (open) {
      document.addEventListener('keydown', onKeydown)
      requestAnimationFrame(() => cancel.value?.focus())
    } else {
      document.removeEventListener('keydown', onKeydown)
    }
  },
)

onBeforeUnmount(() => document.removeEventListener('keydown', onKeydown))
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
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        aria-describedby="confirm-dialog-message"
        class="w-full max-w-sm rounded-3xl border border-gold/15 bg-forest p-6 shadow-2xl"
      >
        <h3 id="confirm-dialog-title" class="font-display text-lg tracking-wide text-gold">
          {{ title }}
        </h3>
        <p id="confirm-dialog-message" class="mt-1 text-sm text-cream/60">
          {{ message }}
        </p>

        <div class="mt-6 flex items-center justify-end gap-3">
          <button
            ref="cancel"
            type="button"
            class="rounded-full px-4 py-1.5 text-sm text-cream/70 transition hover:text-gold"
            @click="emit('close')"
          >
            Cancel
          </button>
          <button
            type="button"
            class="rounded-full bg-clay px-4 py-1.5 text-sm font-medium text-forest transition hover:bg-cream"
            @click="emit('confirm')"
          >
            {{ confirmLabel ?? 'Confirm' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
