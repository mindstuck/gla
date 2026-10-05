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
    // The song list comes later; for now every visit opens the first seeded song.
    { path: '/', redirect: '/songs/1' },
    { path: '/songs/:id', name: 'song', component: () => import('./views/SongView.vue') },
  ],
})

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
