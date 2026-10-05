<script setup lang="ts">
import { onMounted } from "vue"
import { storeToRefs } from "pinia"
import { useSongStore } from "../stores/song"

const store = useSongStore()
const {songs, loading, notFound, error } = storeToRefs(store)
const { loadAll } = store

onMounted(loadAll)
</script>

<template>
    <div class="mx-0">
        <div v-if="loading">loading</div>
        <div v-else-if="notFound">not found</div>
        <div v-else-if="error">{{ error }}</div>
        <div v-else>
            <div v-for="song in songs">
                <span>{{ song.title }}</span> <RouterLink :to="`/songs/${song.id}`">open</RouterLink>
            </div>
        </div>
    </div>
</template>