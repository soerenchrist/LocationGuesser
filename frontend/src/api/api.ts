import { z } from "zod";
import { createZodFetcher } from "zod-fetch";
import { ImageSet, imageSet, image, Image } from "./types";

const fetcher = createZodFetcher();

export async function getImageSets(): Promise<ImageSet[]> {
    const response = await fetcher(z.array(imageSet), `http://localhost:5077/api/imagesets`);
    return response;
}

export async function getGameSet(slug: string): Promise<Image[]> {
    const schema = z.array(image);
    const response = await fetcher(schema, `http://localhost:5077/api/games/${slug}`);
    return response;
}