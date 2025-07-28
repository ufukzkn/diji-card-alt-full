import 'package:flutter/material.dart';
import 'package:forui/forui.dart';

class OtpInput extends StatefulWidget {
  final int boxCount;
  final ValueChanged<String> onChanged;

  const OtpInput({super.key, required this.boxCount, required this.onChanged});

  @override
  State<OtpInput> createState() => _OtpInputState();
}

class _OtpInputState extends State<OtpInput> {
  // Sonradan tanımlama yapılacak kesin bu yüzden "late" kullanıldı
  late List<TextEditingController> _controllers;

  @override
  void initState() {
    super.initState();
    _controllers = List.generate(
      widget.boxCount,
      (_) => TextEditingController(),
    );
    for (final controller in _controllers) {
      controller.addListener(_onAnyChanged);
    }
  }

  @override
  void dispose() {
    for (final controller in _controllers) {
      controller.removeListener(_onAnyChanged);
      controller.dispose();
    }
    super.dispose();
  }

  void _onAnyChanged() {
    final value = _controllers.map((c) => c.text).join();
    widget.onChanged(value);
  }

  @override
  Widget build(BuildContext context) {
    const double spacing = 5.0;
    final double totalSpacing = spacing * (widget.boxCount - 1);
    return LayoutBuilder(
      builder: (context, constraints) {
        final double boxSize =
            (constraints.maxWidth - totalSpacing) / widget.boxCount;
        return Row(
          children: List.generate(widget.boxCount * 2 - 1, (index) {
            if (index.isOdd) {
              return SizedBox(width: spacing);
            }
            final boxIndex = index ~/ 2;
            return SizedBox(
              width: boxSize,
              child: FTextFormField(
                controller: _controllers[boxIndex],
                hint: 'X',
                maxLength: 1,
                textAlign: TextAlign.center,
                keyboardType: TextInputType.number,
              ),
            );
          }),
        );
      },
    );
  }
}
