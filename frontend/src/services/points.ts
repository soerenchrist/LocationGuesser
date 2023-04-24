import { LatLng } from "../api/types";

export function calculateDistance(from: LatLng, to: LatLng) {
  const R = 6371e3;
  const phi1 = (from.lat * Math.PI) / 180;
  const phi2 = (to.lat * Math.PI) / 180;
  const delta1 = ((to.lat - from.lat) * Math.PI) / 180;
  const delta2 = ((to.lng - from.lng) * Math.PI) / 180;

  const a =
    Math.sin(delta1 / 2) * Math.sin(delta1 / 2) +
    Math.cos(phi1) *
      Math.cos(phi2) *
      Math.sin(delta2 / 2) *
      Math.sin(delta2 / 2);

  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

  return R * c;
}

const fullPoints = 5000;
const fullPointsRange = 100;
const noPointsRange = 500 * 1000;

export function calculatePoints(yearsOff: number, distance: number) {
  const distancePoints = Math.round(calculateDistancePoints(distance));
  const yearPoints = Math.round(calculateYearPoints(yearsOff));

  return {
    distancePoints,
    yearPoints,
  };
}

function calculateDistancePoints(distance: number): number {
  if (distance < fullPointsRange) {
    return fullPoints;
  }

  if (distance > noPointsRange) {
    return 0;
  }

  const points =
    (-fullPoints / (noPointsRange - fullPointsRange)) *
      (distance - fullPointsRange) +
    fullPoints;

  return points;
}

function calculateYearPoints(yearsOff: number): number {
  const punishment = 30 * yearsOff * yearsOff;
  return Math.max(0, fullPoints - punishment);
}
