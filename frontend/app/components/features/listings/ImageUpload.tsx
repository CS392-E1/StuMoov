import React, { useState } from "react";
import { supabase } from "@/lib/supabase";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

type ImageUploadProps = {
  onUploadSuccess: (url: string) => void;
};

const ImageUpload: React.FC<ImageUploadProps> = ({ onUploadSuccess }) => {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [imageUrl, setImageUrl] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && !imageUrl) {
      setFile(e.target.files[0]);
    }
  };

  const uploadFile = async () => {
    if (!file || imageUrl) return;
    setUploading(true);

    try {
      const fileExt = file.name.split(".").pop();
      const guid = crypto.randomUUID();
      const filePath = `${guid}.${fileExt}`;

      console.log(`Uploading to: ${filePath}`);

      const { data, error } = await supabase.storage
        .from("images")
        .upload(filePath, file, {
          cacheControl: "3600",
          upsert: false,
        });

      if (error) throw error;

      const {
        data: { publicUrl },
      } = supabase.storage.from("images").getPublicUrl(data.path);

      setImageUrl(publicUrl);
      onUploadSuccess(publicUrl);
      toast.success("Image uploaded successfully!");
    } catch (error) {
      console.error("upload failed", error);
      const errorMsg = error instanceof Error ? error.message : "Unknown error";
      toast.error(`Upload failed: ${errorMsg}`);
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-gray-700">
        Listing Image (Optional, Max 1)
      </label>
      {!imageUrl ? (
        <div className="flex items-center gap-2">
          <Input
            type="file"
            onChange={handleFileChange}
            disabled={uploading || !!imageUrl}
            className="flex-grow"
          />
          <Button
            onClick={uploadFile}
            disabled={uploading || !file || !!imageUrl}
          >
            {uploading ? "Uploading..." : "Upload Image"}
          </Button>
        </div>
      ) : (
        <div className="mt-2">
          <p className="text-sm text-green-600 font-medium">
            Upload successful!
          </p>
          <img
            src={imageUrl}
            alt="Uploaded Listing"
            className="mt-2 max-w-xs max-h-40 rounded border"
          />
        </div>
      )}
    </div>
  );
};

export default ImageUpload;
