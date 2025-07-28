import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:forui/forui.dart';

class ScanScreen extends StatefulWidget {
  const ScanScreen({super.key});

  @override
  State<ScanScreen> createState() => _ScanScreenState();
}

class _ScanScreenState extends State<ScanScreen> {
  MobileScannerController cameraController = MobileScannerController();
  bool _hasPermission = false;
  bool _isScanning = true;

  @override
  void initState() {
    super.initState();
    _requestCameraPermission();
  }

  Future<void> _requestCameraPermission() async {
    final status = await Permission.camera.request();

    if (status.isGranted) {
      setState(() {
        _hasPermission = true;
      });
    } else if (status.isPermanentlyDenied) {
      showFDialog(
        context: context,
        builder:
            (context, style, animation) => FDialog(
              style: style,
              animation: animation,
              direction: Axis.horizontal,
              title: const Text('Kamera İzni Gerekli'),
              body: const Text(
                'Kamera izni kalıcı olarak reddedildi. Ayarlardan manuel olarak izin vermeniz gerekiyor.',
              ),
              actions: [
                FButton(
                  style: FButtonStyle.outline(),
                  onPress: () => {Navigator.of(context).pop()},
                  child: const Text('İptal'),
                ),
                FButton(
                  onPress: () {
                    Navigator.pop(context);
                    openAppSettings();
                  },
                  child: const Text('Devam et'),
                ),
              ],
            ),
      );
    } else {
      setState(() {
        _hasPermission = false;
      });
    }
  }

  void _onDetect(BarcodeCapture capture) {
    if (!_isScanning) return;

    final List<Barcode> barcodes = capture.barcodes;
    for (final barcode in barcodes) {
      if (barcode.rawValue != null) {
        setState(() {
          _isScanning = false;
        });

        debugPrint("Barcode: ${barcode.rawValue}");
        break;
      }
    }
  }

  @override
  void dispose() {
    cameraController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (!_hasPermission) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.camera_alt_outlined, size: 80, color: Colors.grey),
            const SizedBox(height: 24),
            const Text(
              'Kamera İzni Gerekli',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 16),
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 32),
              child: Text(
                'QR kod taramak için kamera iznine ihtiyacımız var.',
                textAlign: TextAlign.center,
                style: TextStyle(fontSize: 16),
              ),
            ),
            const SizedBox(height: 32),
            FButton(
              prefix: Icon(FIcons.camera),
              onPress: _requestCameraPermission,
              child: const Text('İzin ver'),
            ),
          ],
        ),
      );
    }

    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        // Kare şeklinde kamera görüntüsü
        SizedBox(
          width: 300,
          height: 300,
          child: ClipRRect(
            borderRadius: BorderRadius.circular(20),
            child: MobileScanner(
              controller: cameraController,
              onDetect: _onDetect,
            ),
          ),
        ),

        const SizedBox(height: 40),

        // QR ikonu
        const Icon(Icons.qr_code_scanner, size: 60, color: Colors.blue),

        const SizedBox(height: 24),

        // Ana metin
        Text(
          _isScanning
              ? 'QR kodu kare içine yerleştirin'
              : 'QR kod işleniyor...',
          style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w600),
          textAlign: TextAlign.center,
        ),

        const SizedBox(height: 12),

        // Alt metin
        const Text(
          'QR kod otomatik olarak taranacaktır',
          style: TextStyle(fontSize: 16, color: Colors.grey),
          textAlign: TextAlign.center,
        ),
      ],
    );
  }
}
