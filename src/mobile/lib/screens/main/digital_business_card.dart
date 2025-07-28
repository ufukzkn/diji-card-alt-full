import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';
import 'dart:math' as math;

import 'package:stajproje/screens/cards/card_1.dart';
import 'package:stajproje/screens/cards/card_2.dart';

class DigitalBusinessCard extends StatefulWidget {
  final int digitalBusinessCardId;

  const DigitalBusinessCard({super.key, required this.digitalBusinessCardId});

  @override
  State<DigitalBusinessCard> createState() => _DigitalBusinessCardState();
}

class _DigitalBusinessCardState extends State<DigitalBusinessCard> {
  @override
  Widget build(BuildContext context) {
    return FScaffold(
      header: FHeader.nested(
        title: Text('Kartvizit #${widget.digitalBusinessCardId + 1}'),
        prefixes: [
          FHeaderAction.back(
            onPress: () {
              context.go('/main/home');
            },
          ),
        ],
      ),
      child: switch (widget.digitalBusinessCardId) {
        0 => Card1(),
        1 => Card2(),
        _ => const Text('Bu stil henüz desteklenmemektedir.'),
      },
    );
  }
}

// Sabit dalgalı efekt için custom painter
class StaticWavePainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint =
        Paint()
          ..color = Colors.white.withValues(alpha: 0.15)
          ..style = PaintingStyle.fill;

    final path = Path();

    // İlk dalga (sabit)
    path.moveTo(0, size.height * 0.75);

    for (double x = 0; x <= size.width; x++) {
      double y =
          size.height * 0.75 + 25 * math.sin((x / size.width * 2 * math.pi));
      path.lineTo(x, y);
    }

    path.lineTo(size.width, size.height);
    path.lineTo(0, size.height);
    path.close();

    canvas.drawPath(path, paint);

    // İkinci dalga (sabit)
    final paint2 =
        Paint()
          ..color = Colors.white.withValues(alpha: 0.08)
          ..style = PaintingStyle.fill;

    final path2 = Path();
    path2.moveTo(0, size.height * 0.85);

    for (double x = 0; x <= size.width; x++) {
      double y =
          size.height * 0.85 + 15 * math.sin((x / size.width * 3 * math.pi));
      path2.lineTo(x, y);
    }

    path2.lineTo(size.width, size.height);
    path2.lineTo(0, size.height);
    path2.close();

    canvas.drawPath(path2, paint2);
  }

  @override
  bool shouldRepaint(CustomPainter oldDelegate) => false;
}
