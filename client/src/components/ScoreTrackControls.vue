<script setup lang="ts">
import { ref } from 'vue'
import { useDesktopViewport } from '../composables/useDesktopViewport'
import type { TrackControl } from '../types/track'

/**
 * Track selector for the score: a column of instrument buttons over the
 * score's top-left corner (desktop) or a collapsible tray of them above the
 * transport row (mobile), where it rides along with the bottom chrome and
 * hides with it.
 *
 * Clicking a circle mutes/unmutes its track. On desktop the per-track popup
 * (volume, solo, isolate) opens after a ~300ms hover dwell, stays up for a
 * 500ms grace after the pointer leaves, and opens instantly on keyboard
 * focus; mobile shows the same controls inline next to each button — same
 * buttons, same states, only placement differs.
 *
 * Because vertical space is scarce on phones (especially landscape), the
 * mobile tray folds down to a slim strip: tap the strip or swipe down/up
 * over the controls to collapse/expand. Desktop ignores the fold entirely.
 *
 * Presentational only: ScoreViewport owns the alphaTab API and the track
 * state this renders — intents come back as events.
 */
const props = defineProps<{
  tracks: TrackControl[]
}>()

const emit = defineEmits<{
  mute: [index: number]
  solo: [index: number]
  volume: [index: number, volume: number]
  open: [index: number]
}>()

/** Track whose popup is currently open (desktop hover / keyboard focus). */
const openIndex = ref<number | null>(null)
let openTimer: ReturnType<typeof setTimeout> | undefined
let closeTimer: ReturnType<typeof setTimeout> | undefined

/** Dwell before the popup opens so passing over the buttons never flashes it. */
const OPEN_DELAY_MS = 300
/** Grace after leaving, so a small slip off the button doesn't kill the popup. */
const CLOSE_DELAY_MS = 500

/**
 * Desktop = wide *and* tall; short viewports (landscape phones, small
 * split windows) keep the mobile tray even when they are wide. The query
 * lives in the shared composable, in sync with the `desktop` variant.
 */
const isDesktop = useDesktopViewport()

/** Mobile only: the tray collapses onto its strip unless the user opens it. */
const folded = ref(true)
/** Vertical travel (px) before a touch counts as a fold/unfold swipe. */
const SWIPE_THRESHOLD_PX = 44
let touchStartY: number | null = null

function setFolded(next: boolean): void {
  folded.value = next
  // Collapsing must not leave a popup open behind the hidden tray.
  if (next) closeNow()
}

function toggleFold(): void {
  setFolded(!folded.value)
}

function onTouchStart(event: TouchEvent): void {
  touchStartY = event.touches[0]?.clientY ?? null
}

function onTouchEnd(event: TouchEvent): void {
  const startY = touchStartY
  touchStartY = null
  if (startY === null || isDesktop.value) return
  const endY = event.changedTouches[0]?.clientY ?? startY
  const deltaY = endY - startY
  if (Math.abs(deltaY) < SWIPE_THRESHOLD_PX) return
  // Swiping down folds the tray away; swiping up raises it again.
  if (deltaY > 0) {
    if (!folded.value) setFolded(true)
  } else if (folded.value) {
    setFolded(false)
  }
}

function clearOpenTimer(): void {
  if (openTimer !== undefined) {
    clearTimeout(openTimer)
    openTimer = undefined
  }
}

function clearCloseTimer(): void {
  if (closeTimer !== undefined) {
    clearTimeout(closeTimer)
    closeTimer = undefined
  }
}

function closeNow(): void {
  clearOpenTimer()
  clearCloseTimer()
  openIndex.value = null
}

/** Hide after the grace period — unless focus is still inside (slider drag);
 *  focusout schedules the real close when focus eventually moves on. */
function scheduleClose(wrapper: HTMLElement): void {
  clearOpenTimer()
  clearCloseTimer()
  closeTimer = setTimeout(() => {
    closeTimer = undefined
    if (!wrapper.contains(document.activeElement)) {
      openIndex.value = null
    }
  }, CLOSE_DELAY_MS)
}

function onMouseEnter(index: number): void {
  clearCloseTimer()
  clearOpenTimer()
  if (openIndex.value === index) {
    return // already showing this track's controls
  }
  if (openIndex.value !== null) {
    // Popup is up — moving along the column switches it at once.
    openIndex.value = index
    return
  }
  openTimer = setTimeout(() => {
    openTimer = undefined
    openIndex.value = index
  }, OPEN_DELAY_MS)
}

function onMouseLeave(event: MouseEvent): void {
  scheduleClose(event.currentTarget as HTMLElement)
}

/** Keyboard focus opens the popup without the dwell — it must be tab-reachable. */
function onFocusIn(index: number): void {
  clearCloseTimer()
  clearOpenTimer()
  openIndex.value = index
}

function onFocusOut(event: FocusEvent, index: number): void {
  const wrapper = event.currentTarget as HTMLElement
  const next = event.relatedTarget
  if (next instanceof Node && wrapper.contains(next)) {
    return // moving between this track's own controls keeps the popup open
  }
  if (openIndex.value === index) {
    scheduleClose(wrapper)
  }
}

function onEscape(): void {
  closeNow()
}

function onVolumeInput(index: number, event: Event): void {
  emit('volume', index, Number((event.target as HTMLInputElement).value))
}

/** True when the score view shows this track and nothing else. */
function isIsolated(track: TrackControl): boolean {
  return (
    track.rendered &&
    props.tracks.every((other) => other.index === track.index || !other.rendered)
  )
}

/** The circle: solid gray when muted, gold while part of the rendered view. */
function buttonClass(track: TrackControl): string {
  const base =
    'flex h-9 w-9 items-center justify-center rounded-full border-2 transition'
  if (track.muted) {
    return `${base} border-neutral-500 bg-neutral-400 text-neutral-700 hover:border-neutral-600`
  }
  if (track.rendered) {
    return `${base} border-forest bg-gold text-forest hover:bg-cream`
  }
  return `${base} border-gold/50 bg-tartan text-cream hover:bg-forest hover:text-gold`
}

/** Solo and isolate share one small-button style; gold fill when active. */
function smallButtonClass(active: boolean): string {
  const base =
    'flex h-7 w-7 items-center justify-center rounded-full border text-[11px] font-bold leading-none transition'
  return active
    ? `${base} border-gold bg-gold text-forest`
    : `${base} border-cream/40 bg-forest text-cream hover:border-gold hover:text-gold`
}

/** Popup: inline on mobile, delayed floating panel on desktop. */
function popupClass(track: TrackControl): string {
  const base =
    'flex items-center gap-2 transition-opacity desktop:absolute desktop:left-full desktop:top-0 desktop:ml-1 desktop:rounded-lg desktop:border desktop:border-gold/40 desktop:bg-tartan desktop:px-2.5 desktop:py-1.5 desktop:shadow-lg'
  const open = openIndex.value === track.index
  return open
    ? `${base} desktop:visible desktop:opacity-100 desktop:pointer-events-auto`
    : `${base} desktop:invisible desktop:opacity-0 desktop:pointer-events-none`
}
</script>

<template>
  <!-- On desktop the column floats over the score's top-left corner. Its
       containing block is now the ScoreViewport root (the tray moved into
       the bottom chrome wrapper, which is static here), so the offsets
       reproduce the old panel-relative inset: panel margin 1rem + its
       dashed border 4px + 0.25rem = 1.5rem. -->
  <div
    v-if="tracks.length > 0"
    role="group"
    aria-label="Track controls"
    class="flex flex-col items-start gap-2 px-3 pb-2 pt-2 desktop:absolute desktop:left-6 desktop:top-6 desktop:z-20 desktop:gap-1.5 desktop:p-0"
    @touchstart.passive="onTouchStart"
    @touchend="onTouchEnd"
  >
    <!-- Fold/unfold handle: the mobile tray collapses onto this slim strip
         so the score keeps its space; swiping down/up works too. -->
    <button
      v-show="!isDesktop"
      type="button"
      class="flex h-7 w-full items-center justify-center rounded-full border border-gold/40 bg-tartan text-gold transition active:opacity-70"
      :aria-expanded="!folded"
      :aria-label="folded ? 'Show track controls' : 'Hide track controls'"
      :title="folded ? 'Show track controls' : 'Hide track controls'"
      @click="toggleFold"
    >
      <svg
        class="h-4 w-4 transition-transform duration-200"
        :class="folded ? 'rotate-180' : ''"
        viewBox="0 0 16 16"
        fill="none"
        stroke="currentColor"
        stroke-width="1.8"
        stroke-linecap="round"
        stroke-linejoin="round"
        aria-hidden="true"
      >
        <path d="M4 6l4 4 4-4" />
      </svg>
    </button>

    <div
      v-for="track in tracks"
      :key="track.index"
      v-show="isDesktop || !folded"
      class="group relative flex items-center gap-2"
      @mouseenter="onMouseEnter(track.index)"
      @mouseleave="onMouseLeave"
      @focusin="onFocusIn(track.index)"
      @focusout="onFocusOut($event, track.index)"
      @keydown.esc="onEscape"
    >
      <!-- Mute toggle. The mousedown prevent keeps a pointer click from
           focusing the button: the popup belongs to hover dwell and keyboard
           focus, not to the mute action itself. -->
      <button
        type="button"
        :class="buttonClass(track)"
        :aria-pressed="track.muted"
        :aria-label="track.muted ? `Unmute ${track.name}` : `Mute ${track.name}`"
        :title="track.muted ? `Unmute ${track.name}` : `Mute ${track.name}`"
        @mousedown.prevent
        @click="emit('mute', track.index)"
      >
        <!-- Drums: drum head + shell, sticks crossed above. -->
        <svg
          v-if="track.kind === 'drums'"
          class="h-4 w-4"
          viewBox="0 0 16 16"
          fill="none"
          stroke="currentColor"
          stroke-width="1.4"
          aria-hidden="true"
        >
          <ellipse cx="8" cy="10.4" rx="5.4" ry="2.3" fill="currentColor" stroke="none" />
          <path d="M2.6 10.4v2.4c0 1.3 2.4 2.3 5.4 2.3s5.4-1 5.4-2.3v-2.4" />
          <path d="M2.5 1.6 9.8 6.2M13.5 1.6 6.2 6.2" stroke-linecap="round" />
        </svg>

        <!-- Guitar: body with sound hole, neck, headstock. -->
        <svg v-else-if="track.kind === 'guitar'" class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
          <path
            fill-rule="evenodd"
            clip-rule="evenodd"
            d="M3.8 11.1a4.2 4.6 0 1 0 8.4 0 4.2 4.6 0 1 0-8.4 0ZM6.5 11.6a1.5 1.5 0 1 0 3 0 1.5 1.5 0 1 0-3 0Z"
          />
          <rect x="7.1" y="2.6" width="1.8" height="5.6" rx="0.4" />
          <rect x="6.1" y="0.9" width="3.8" height="2.1" rx="0.7" />
        </svg>

        <!-- Bass: long horizontal neck with solid offset body. -->
        <svg v-else-if="track.kind === 'bass'" class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
          <ellipse cx="11.6" cy="9.6" rx="3.9" ry="4.4" />
          <rect x="1.4" y="8.8" width="7.2" height="1.6" rx="0.5" />
          <rect x="0.6" y="7.4" width="2" height="4.4" rx="0.7" />
        </svg>

        <!-- Violin: figure-8 body with scroll, crossed by the bow. -->
        <svg
          v-else-if="track.kind === 'violin'"
          class="h-4 w-4"
          viewBox="0 0 16 16"
          fill="none"
          stroke="currentColor"
          stroke-width="1.3"
          aria-hidden="true"
        >
          <ellipse cx="7" cy="11.4" rx="3.3" ry="3.1" fill="currentColor" stroke="none" />
          <ellipse cx="7" cy="6.8" rx="2.5" ry="2.3" fill="currentColor" stroke="none" />
          <path d="M7 4.5V2.2" stroke-linecap="round" />
          <circle cx="7" cy="1.6" r="1" fill="currentColor" stroke="none" />
          <path d="M1.4 14.4 14.6 3.4" stroke-linecap="round" />
        </svg>

        <!-- Piano: keyboard with black keys. -->
        <svg v-else-if="track.kind === 'piano'" class="h-4 w-4" viewBox="0 0 16 16" fill="none" aria-hidden="true">
          <rect x="1.8" y="4.2" width="12.4" height="7.6" rx="1.2" stroke="currentColor" stroke-width="1.5" />
          <path d="M5.4 4.4v3.4M10.6 4.4v3.4" stroke="currentColor" stroke-width="1.5" />
        </svg>

        <!-- Fallback: eighth note. -->
        <svg v-else class="h-4 w-4" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
          <ellipse cx="4.8" cy="12.2" rx="2.7" ry="2.2" transform="rotate(-15 4.8 12.2)" />
          <rect x="7" y="2.4" width="1.3" height="9.8" />
          <path d="M8.3 2.4c2.6.5 3.9 2.4 3.6 5.2-.6-1.9-1.7-2.9-3.6-3.2z" />
        </svg>
      </button>

      <div
        :class="popupClass(track)"
        role="group"
        :aria-label="`Controls for ${track.name}`"
      >
        <!-- Desktop only: the mobile row relies on the button order instead. -->
        <span class="hidden max-w-20 truncate text-[10px] font-semibold uppercase tracking-wider text-cream/70 desktop:inline">
          {{ track.name }}
        </span>

        <input
          type="range"
          min="0"
          max="1"
          step="0.05"
          :value="track.volume"
          :aria-label="`Volume for ${track.name}`"
          class="h-4 w-20 cursor-pointer accent-gold"
          @input="onVolumeInput(track.index, $event)"
        />

        <button
          type="button"
          :class="smallButtonClass(track.solo)"
          :aria-pressed="track.solo"
          :aria-label="`Solo ${track.name}`"
          :title="`Solo ${track.name}`"
          @click="emit('solo', track.index)"
        >
          <span aria-hidden="true">S</span>
        </button>

        <button
          type="button"
          :class="smallButtonClass(isIsolated(track))"
          :aria-pressed="isIsolated(track)"
          :aria-label="`Show ${track.name}`"
          :title="`Show ${track.name}`"
          @click="emit('open', track.index)"
        >
          <svg
            class="h-3.5 w-3.5"
            viewBox="0 0 16 16"
            fill="none"
            stroke="currentColor"
            stroke-width="1.6"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
          >
            <path d="M6 2.5H2.5V6M10 2.5h3.5V6M13.5 10v3.5H10M6 13.5H2.5V10" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>
