<script setup lang="ts">
import type { ImageSet } from '../../api/types';

defineProps<{
    imageSet: ImageSet
}>();

defineEmits

</script>

<template>
    <router-link to="/game">
        <div class="rounded-lg overflow-hidden shadow-lg bg-teal-600 hover:bg-teal-700 cursor-pointer">
            <div class="px-6 py-4">
                <div class="font-bold text-xl mb-2">{{ imageSet.title }}</div>
                <p class="text-gray-200 text-base">
                    {{ imageSet.description }}
                </p>
            </div>
            <div class="px-6 pt-4 pb-2">
                <template v-for="tag in imageSet.tags.split(',')">
                    <span class="inline-block bg-gray-200 rounded-full px-3 py-1 text-sm font-semibold text-gray-700 mr-2 mb-2">
                        {{ tag }}
                    </span>
                </template>
            </div>
        </div>
    </router-link>
</template>