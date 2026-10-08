import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
// Header wordmark (Black Ops One, latin subset, weight 400 only).
import '@fontsource/black-ops-one/latin-400.css'
import './style.css'
import { createApp } from 'vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    // The songs list is the app's entry point; songs open from it.
    { path: '/', redirect: '/songs' },
    {
      path: '/songs',
      name: 'songs',
      component: () => import('./views/SongsView.vue'),
      // The list has no score under the bars, so they stay up here.
      meta: { chrome: 'pinned' },
    },
    { path: '/songs/:id', name: 'song', component: () => import('./views/SongView.vue') },
  ],
})

/**
 * Per-page chrome behaviour: a page declares whether the bars stay pinned
 * (never slide away) or collapse on their own. Pages that say nothing get
 * the collapsible default; desktop pins regardless (see App.vue).
 */
declare module 'vue-router' {
  interface RouteMeta {
    chrome?: import('./stores/chrome').ChromeMode
  }
}

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
