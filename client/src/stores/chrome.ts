import { defineStore } from 'pinia'
import { ref } from 'vue'

/** Idle time without a tap before the chrome slides away. */
const AUTO_HIDE_MS = 3000

/**
 * How a page uses the chrome.
 *
 * - `'pinned'` — the bars belong to that page and stay: no countdown runs and
 *   neither taps nor `dismiss()` can take them away. Desktop pins this way
 *   always, and any route may opt in with `meta: { chrome: 'pinned' }`.
 * - `'collapsible'` — the mobile default: the bars auto-hide after the idle
 *   countdown and come back on a tap.
 */
export type ChromeMode = 'pinned' | 'collapsible'

/**
 * Visibility of the overlaying chrome (header, bottom stack) plus the bottom
 * stack's expanded state.
 *
 * The mobile full-bleed layout lets the score fill the whole screen, so those
 * bars float on top of it and slide out of the way when reading — mobile
 * YouTube style. Taps are the only interaction that moves them: a tap on the
 * score toggles the chrome, a tap on the chrome itself keeps it up (and
 * restarts the countdown) so pressing a control never hides the controls —
 * with one deliberate exception, `dismiss()`, which Play uses to take them
 * away at once.
 *
 * The footer expands onto the track controls (swipe up, or the arrow in its
 * middle) and that expansion pins the chrome on its own: while the panel is
 * open neither the idle countdown nor a tap takes it away, since the controls
 * would vanish mid-use. Closing it hands the countdown back.
 *
 * The bars are always present on desktop, which pins them: there the score is
 * not full-bleed, nothing ever hides and the footer never expands. A page may
 * pin itself the same way on any viewport through its route's `chrome` meta
 * (App.vue maps that — and the viewport — onto `setMode`), so a page that
 * wants the bars out of the way keeps the collapsible default while one that
 * doesn't (the songs list) asks for pinned.
 */
export const useChromeStore = defineStore('chrome', () => {
  /** What the current page asked for; applied through `setMode`. */
  const mode = ref<ChromeMode>('collapsible')
  /** Chrome visibility. `true` whenever it is pinned (desktop) or armed. */
  const visible = ref(true)
  /** Bottom chrome opened onto the track controls — mobile only. */
  const expanded = ref(false)

  let hideTimer: ReturnType<typeof setTimeout> | undefined

  function clearTimer(): void {
    if (hideTimer !== undefined) {
      clearTimeout(hideTimer)
      hideTimer = undefined
    }
  }

  /** (Re)starts the idle countdown that hides the chrome. */
  function arm(): void {
    clearTimer()
    if (mode.value === 'pinned' || expanded.value) {
      // pinned pages never hide; an open footer pins the chrome by itself
      return
    }
    hideTimer = setTimeout(() => {
      hideTimer = undefined
      visible.value = false
    }, AUTO_HIDE_MS)
  }

  /** Chrome stays on screen: no countdown running, nothing ever hides.
   *  It also drops any expansion — the pinned pages (desktop, the songs list)
   *  show their footer as a plain row. */
  function pin(): void {
    clearTimer()
    expanded.value = false
    visible.value = true
  }

  /**
   * Applies a page's chrome behaviour — the single entry point for choosing
   * between pinned and collapsible (App.vue feeds it the viewport and the
   * route's `chrome` meta).
   *
   * Switching to pinned raises the bars at once and leaves them there;
   * switching back hands the idle countdown over, so they fade on their own
   * again. Everything that could hide the bars — taps, the countdown,
   * `dismiss()` — consults `mode`, so a pinned page stays pinned no matter
   * what fires.
   */
  function setMode(next: ChromeMode): void {
    mode.value = next
    if (next === 'pinned') {
      pin()
    } else {
      arm()
    }
  }

  /** Keeps the chrome up for the duration of the current gesture. */
  function wake(): void {
    visible.value = true
    clearTimer()
  }

  /**
   * A tap landed.
   *
   * @param onChrome true when the tap hit the chrome — header, track tray or
   *                 footer — which keeps it up instead of toggling it.
   */
  function tap(onChrome: boolean): void {
    if (mode.value === 'pinned') {
      return // this page keeps its bars — a tap changes nothing
    }
    if (expanded.value) {
      return // the open track panel holds everything up until it closes
    }
    if (onChrome) {
      // Keeping it up means restarting the countdown — but only while it is
      // up. The bars are inert when hidden, so a tap on the chrome normally
      // implies `visible`; forcing it back on would undo the dismissal that a
      // control just made (Play hides the chrome, this click follows it).
      if (visible.value) {
        arm()
      }
      return
    }
    visible.value = !visible.value
    if (visible.value) {
      arm()
    } else {
      clearTimer()
    }
  }

  /**
   * Hides the chrome now: no countdown left running, no open track panel left
   * behind. Playback starting is what calls it — the score is what matters
   * then, and the bars would otherwise sit over it for another idle period.
   *
   * Pinned pages refuse it: nothing on them would ever bring a dismissed bar
   * back, so `mode` gets the final word (Play only ever runs on a collapsible
   * one anyway).
   */
  function dismiss(): void {
    if (mode.value === 'pinned') {
      return // this page keeps its bars
    }
    clearTimer()
    expanded.value = false
    visible.value = false
  }

  /** Opens the footer onto the track controls: chrome up, countdown off. */
  function expand(): void {
    expanded.value = true
    visible.value = true
    clearTimer()
  }

  /** Closes it again — the idle countdown resumes. */
  function collapse(): void {
    if (!expanded.value) {
      return
    }
    expanded.value = false
    arm()
  }

  return { mode, visible, expanded, setMode, arm, pin, wake, tap, dismiss, expand, collapse }
})
