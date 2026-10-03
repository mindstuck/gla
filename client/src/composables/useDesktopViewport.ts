import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue'

/**
 * Viewport class shared by the CSS (the `desktop`/`mobile` custom variants in
 * style.css) and the script branches: wide AND tall is the roomy desktop
 * layout, everything else — landscape phones, small split windows — is mobile.
 *
 * Keep this query in sync with the `desktop` custom variant in style.css.
 */
export const DESKTOP_MEDIA = '(min-width: 768px) and (min-height: 600px)'

/** Reactive desktop flag, updated whenever the viewport crosses the query. */
export function useDesktopViewport(): Ref<boolean> {
  const mediaQuery = window.matchMedia(DESKTOP_MEDIA)
  const isDesktop = ref(mediaQuery.matches)

  function onViewportChange(event: MediaQueryListEvent): void {
    isDesktop.value = event.matches
  }

  onMounted(() => mediaQuery.addEventListener('change', onViewportChange))
  onBeforeUnmount(() => mediaQuery.removeEventListener('change', onViewportChange))

  return isDesktop
}
