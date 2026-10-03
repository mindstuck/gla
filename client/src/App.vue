<template>
  <!-- h-dvh (not min-h-screen): a definite viewport height makes the
       flex-1/min-h-0 chain resolve, so the score viewport scrolls internally
       and the footer stays pinned instead of the document growing.
       overflow-hidden clips the chrome while it slides off the edges. -->
  <div class="flex h-dvh flex-col overflow-hidden bg-forest text-gold">
    <!-- Mobile: the header overlays the score's top edge (absolute, so hiding
         it never resizes the score) and covers the notch area, which
         .safe-area-top keeps clear of its content. Desktop keeps it in flow. -->
    <header
      data-chrome
      class="border-b border-gold/15 safe-area-top transition-transform duration-300 ease-out mobile:absolute mobile:inset-x-0 mobile:top-0 mobile:z-40 mobile:bg-forest"
      :class="{ 'mobile:-translate-y-full': !chrome.visible }"
      :inert="!chrome.visible"
    >
      <div class="px-6 py-4">
        <h1 class="text-xl font-semibold tracking-wide">GLA</h1>
      </div>
    </header>

    <main class="mx-auto flex min-h-0 w-full max-w-[92rem] flex-1 flex-col bg-bottle">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, watch } from 'vue'
import { useDesktopViewport } from './composables/useDesktopViewport'
import { useChromeStore } from './stores/chrome'

const chrome = useChromeStore()
const isDesktop = useDesktopViewport()

/** Taps are the only interaction: on the score they toggle the chrome,
 *  on the chrome itself they keep it up and restart the countdown. */
function onTap(event: MouseEvent): void {
  if (isDesktop.value || !(event.target instanceof Element)) return
  chrome.tap(event.target.closest('[data-chrome]') !== null)
}

/** A gesture starting on the chrome (a slider drag, say) holds it open —
 *  the finger may well stay down longer than the idle delay. */
function onPointerDown(event: PointerEvent): void {
  if (isDesktop.value || !(event.target instanceof Element)) return
  if (event.target.closest('[data-chrome]')) {
    chrome.wake()
  }
}

/** The countdown restarts when the finger lifts; the `click` that follows
 *  (the tap itself) then decides by toggling. */
function onPointerUp(): void {
  if (isDesktop.value) return
  chrome.arm()
}

onMounted(() => {
  document.addEventListener('click', onTap)
  document.addEventListener('pointerdown', onPointerDown)
  document.addEventListener('pointerup', onPointerUp)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onTap)
  document.removeEventListener('pointerdown', onPointerDown)
  document.removeEventListener('pointerup', onPointerUp)
})

// Desktop pins the chrome; mobile arms the countdown — on load and whenever
// the viewport stops being desktop.
watch(
  isDesktop,
  (desktop) => (desktop ? chrome.pin() : chrome.arm()),
  { immediate: true },
)
</script>
