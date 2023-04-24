<script setup lang="ts">
defineProps({
    title: String,
    value: String
})
</script>

<template>
    <div class="rounded-lg shadow-lg bg-teal-600">
        <div class="px-6 py-4">
            <p class="font-bold text-2xl mb-2">{{ value }}</p>
            <p class="text-gray-200 text-base">
                {{ title }}
            </p>
        </div>
    </div>
</template>