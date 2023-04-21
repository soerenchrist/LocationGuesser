import { z } from "zod";
import { createZodFetcher } from "zod-fetch";
import { ImageSet, imageSet } from "./types";

const fetcher = createZodFetcher();

export async function getImageSets(): Promise<ImageSet[]> {
    const response = await fetcher(z.array(imageSet), `http://localhost:5077/api/imagesets`);
    return response;
}