import { defineStore } from 'pinia'
import { ref } from 'vue'

/** Idle time without a tap before the chrome slides away. */
const AUTO_HIDE_MS = 2500

/**
 * Visibility of the overlaying chrome (header, bottom stack) plus the bottom
 * stack's expanded state.
 *
 * The mobile full-bleed layout lets the score fill the whole screen, so those
 * bars float on top of it and slide out of the way when reading — mobile
 * YouTube style. Taps are the only interaction that moves them: a tap on the
 * score toggles the chrome, a tap on the chrome itself keeps it up (and
 * restarts the countdown) so pressing a control never hides the controls.
 *
 * The footer expands onto the track controls (swipe up, or the arrow in its
 * middle) and that expansion pins the chrome on its own: while the panel is
 * open neither the idle countdown nor a tap takes it away, since the controls
 * would vanish mid-use. Closing it hands the countdown back.
 *
 * The bars are always present on desktop, which pins them: there the score is
 * not full-bleed, nothing ever hides and the footer never expands.
 */
export const useChromeStore = defineStore('chrome', () => {
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
    if (expanded.value) {
      return // an open footer pins the chrome by itself
    }
    hideTimer = setTimeout(() => {
      hideTimer = undefined
      visible.value = false
    }, AUTO_HIDE_MS)
  }

  /** Chrome stays on screen: no countdown running, nothing ever hides.
   *  Desktop also drops any expansion — there the footer is a plain row. */
  function pin(): void {
    clearTimer()
    expanded.value = false
    visible.value = true
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
    if (expanded.value) {
      return // the open track panel holds everything up until it closes
    }
    if (onChrome) {
      visible.value = true
      arm()
      return
    }
    visible.value = !visible.value
    if (visible.value) {
      arm()
    } else {
      clearTimer()
    }
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

  return { visible, expanded, arm, pin, wake, tap, expand, collapse }
})
