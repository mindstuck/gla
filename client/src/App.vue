<template>
  <div class="flex min-h-screen flex-col bg-forest text-gold">
    <header class="border-b border-gold/15 px-6 py-4">
      <h1 class="text-xl font-semibold tracking-wide">GLA</h1>
    </header>

    <main class="mx-auto flex min-h-0 w-full max-w-[92rem] flex-1 flex-col bg-bottle">
      <router-view />
    </main>
  </div>
</template>
