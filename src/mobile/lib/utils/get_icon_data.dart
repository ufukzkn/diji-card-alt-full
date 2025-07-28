import 'package:flutter/material.dart';

IconData? getIconDataFromString(String? input) {
  if (input == null) return null;

  // Regex ile U+ ayırma
  final regex = RegExp(r'U\+([0-9A-Fa-f]+)');
  final match = regex.firstMatch(input);

  if (match != null) {
    final hexCode = match.group(1);
    final codePoint = int.tryParse(hexCode!, radix: 16);
    if (codePoint != null) {
      return IconData(codePoint, fontFamily: 'MaterialIcons');
    }
  }

  return null;
}
