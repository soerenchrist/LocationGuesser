<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import * as L from 'leaflet';
import { onMounted, ref, watch } from 'vue';
import { LatLng } from '../../api/types';

const map = ref<L.Map>();
const selectedPositionMarker = ref<L.Marker>();
const correctPositionMarker = ref<L.Marker>();
const line = ref<L.Polyline>();

const emit = defineEmits<{
    (e: 'click', latLng: LatLng): void
}>()

const props = defineProps<{
    selectedPosition?: { lat: number, lng: number },
    showResult: boolean,
    correctPosition?: { lat: number, lng: number }
}>();

const onMapClick = (e: any) => {
    emit('click', e.latlng);
}

const displayCorrectPositionMarker = (position: LatLng | undefined, showResult: boolean) => {
    if (!position || !showResult) {
        if (correctPositionMarker.value) {
            map.value!.removeLayer(correctPositionMarker.value);
            correctPositionMarker.value = undefined;
        }

        return;
    }
    if (!correctPositionMarker.value) {
        correctPositionMarker.value = L.marker(position).addTo(map.value!);
    }
    correctPositionMarker.value.setLatLng(position);
}

const displayPolyline = (correctPosition: LatLng | undefined, selectedPosition: LatLng | undefined, showResult: boolean) => {
    if (!correctPosition || !selectedPosition || !showResult) {
        if (line.value) {
            map.value!.removeLayer(line.value);
            line.value = undefined;
        }

        return;
    }
    if (!line.value) {
        line.value = L.polyline([correctPosition, selectedPosition], {
            color: "red",
            lineCap: "round",
            weight: 2,
            dashArray: "3"
        }).addTo(map.value!);
    }
    line.value.setLatLngs([correctPosition, selectedPosition]);
}

const displayPositionMarker = (position?: LatLng) => {
    if (!position) {
        if (selectedPositionMarker.value) {
            map.value!.removeLayer(selectedPositionMarker.value);
            selectedPositionMarker.value = undefined;
        }

        return;
    }
    if (!selectedPositionMarker.value) {
        selectedPositionMarker.value = L.marker(position).addTo(map.value!);
    }
    selectedPositionMarker.value.setLatLng(position);
}

watch(props, (newProps) => {
    displayPositionMarker(newProps.selectedPosition);
    displayCorrectPositionMarker(newProps.correctPosition, newProps.showResult);
    displayPolyline(newProps.correctPosition, newProps.selectedPosition, newProps.showResult);

    if (newProps.showResult) {
        const group = L.featureGroup([selectedPositionMarker.value!, correctPositionMarker.value!]);
        map.value!.fitBounds(group.getBounds());
    }
});

onMounted(() => {
    map.value = L.map('map').setView([45.505, -0], 3);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map.value);
    map.value.on('click', onMapClick);
})

</script>

<template>
    <div style="height: 600px; width: 100%">
        <div id="map" class="w-full h-full"></div>
    </div>
</template>