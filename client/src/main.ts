import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import App from './App.vue'
import './style.css'
import { createApp } from 'vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    // Temporary until the song page exists (Step 5).
    { path: '/', name: 'home', component: () => import('./views/HomeView.vue') },
  ],
})

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
