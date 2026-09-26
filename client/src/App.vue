<template>
  <!-- h-dvh (not min-h-screen): a definite viewport height makes the
       flex-1/min-h-0 chain resolve, so the score viewport scrolls internally
       and the footer stays pinned instead of the document growing. -->
  <div class="flex h-dvh flex-col bg-forest text-gold">
    <header class="border-b border-gold/15 px-6 py-4">
      <h1 class="text-xl font-semibold tracking-wide">GLA</h1>
    </header>

    <main class="mx-auto flex min-h-0 w-full max-w-[92rem] flex-1 flex-col bg-bottle">
      <router-view />
    </main>
  </div>
</template>
