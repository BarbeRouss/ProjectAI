"use client";

import { useTranslations } from "next-intl";
import { useDeleteHouse } from "@/lib/api/hooks";
import { useRouter } from "next/navigation";
import { useLocale } from "next-intl";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AlertTriangle } from "lucide-react";

interface DeleteHouseDialogProps {
  houseId: string;
  houseName: string;
  open: boolean;
  onClose: () => void;
}

export function DeleteHouseDialog({ houseId, houseName, open, onClose }: DeleteHouseDialogProps) {
  const t = useTranslations("houses");
  const tCommon = useTranslations("common");
  const router = useRouter();
  const locale = useLocale();

  const deleteMutation = useDeleteHouse({
    onSuccess: () => {
      router.push(`/${locale}/dashboard`);
    },
  });

  const handleDelete = () => {
    deleteMutation.mutate(houseId);
  };

  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-red-600 dark:text-red-400">
            <AlertTriangle className="h-5 w-5" />
            {t("deleteHouse")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-600 dark:text-gray-400 mb-6">
            {t("deleteConfirmation", { name: houseName })}
          </p>

          {deleteMutation.isError && (
            <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 px-4 py-3 rounded text-sm mb-4">
              {tCommon("error")}
            </div>
          )}

          <div className="flex gap-3">
            <Button type="button" variant="outline" onClick={onClose} className="flex-1">
              {tCommon("cancel")}
            </Button>
            <Button
              type="button"
              variant="destructive"
              onClick={handleDelete}
              disabled={deleteMutation.isPending}
              className="flex-1"
            >
              {deleteMutation.isPending ? tCommon("loading") : tCommon("delete")}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
