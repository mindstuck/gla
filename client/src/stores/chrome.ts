import { defineStore } from 'pinia'
import { ref } from 'vue'

/** Idle time without a tap before the chrome slides away. */
const AUTO_HIDE_MS = 2500

/**
 * Visibility of the overlaying chrome (header, track tray, footer).
 *
 * The mobile full-bleed layout lets the score fill the whole screen, so those
 * bars float on top of it and slide out of the way when reading — mobile
 * YouTube style. Taps are the only interaction that moves them: a tap on the
 * score toggles the chrome, a tap on the chrome itself keeps it up (and
 * restarts the countdown) so pressing a control never hides the controls.
 *
 * The bars are always present on desktop, which pins them: there the score is
 * not full-bleed and nothing ever hides.
 */
export const useChromeStore = defineStore('chrome', () => {
  /** Chrome visibility. `true` whenever it is pinned (desktop) or armed. */
  const visible = ref(true)

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
    hideTimer = setTimeout(() => {
      hideTimer = undefined
      visible.value = false
    }, AUTO_HIDE_MS)
  }

  /** Chrome stays on screen: no countdown running, nothing ever hides. */
  function pin(): void {
    clearTimer()
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

  return { visible, arm, pin, wake, tap }
})
