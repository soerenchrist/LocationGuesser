<script setup lang="ts">
import { ref } from 'vue';

const currentSlug = ref(window.location.hash);
console.log(currentSlug);

</script>

<template>
  <h1>Game</h1>
</template>