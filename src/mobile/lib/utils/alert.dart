import 'package:flutter/material.dart';
import 'package:forui/forui.dart';

enum AlertType { success, error, info }

void showCustomToast({
  required BuildContext context,
  required String title,
  required String message,
  required AlertType type,
  Duration duration = const Duration(seconds: 3),
}) {
  switch (type) {
    case AlertType.success:
      showFToast(
        icon: const Icon(FIcons.check),
        context: context,
        title: Text(title),
        description: Text(message),
        duration: duration,
        swipeToDismiss: [AxisDirection.right, AxisDirection.down],
      );
      break;
    case AlertType.error:
      showFToast(
        icon: const Icon(FIcons.x),
        context: context,
        title: Text(title),
        description: Text(message),
        duration: duration,
        swipeToDismiss: [AxisDirection.right, AxisDirection.down],
      );
      break;
    case AlertType.info:
      showFToast(
        icon: const Icon(FIcons.info),
        context: context,
        title: Text(title),
        description: Text(message),
        duration: duration,
        swipeToDismiss: [AxisDirection.right, AxisDirection.down],
      );
      break;
  }
}
