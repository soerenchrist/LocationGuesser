<script setup lang="ts">
defineProps({
    title: String
});
</script>

<template>
    <div>
        <h1 class="text-7xl font-extrabold pt-8">{{ title }}</h1>
    </div>
</template>