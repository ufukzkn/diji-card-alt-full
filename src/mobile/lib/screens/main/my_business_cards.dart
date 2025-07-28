import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:stajproje/providers/theme_provider.dart';

class MyBusinessCards extends StatefulWidget {
  const MyBusinessCards({super.key});

  @override
  State<MyBusinessCards> createState() => _MyBusinessCardsState();
}

class _MyBusinessCardsState extends State<MyBusinessCards> {
  @override
  Widget build(BuildContext context) {
    final themeProvider = context.watch<ThemeProvider>();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        SizedBox(height: 50),
        SizedBox(
          width: 125,
          child: FButton(
            prefix: Icon(FIcons.plus),
            onPress: () {
              context.go('/new-business-card');
            },
            child: const Text('Yeni'),
          ),
        ),
        SizedBox(height: 12),
        Expanded(
          child: GridView.builder(
            padding: EdgeInsets.only(bottom: 12.0),
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 2,
              crossAxisSpacing: 12,
              mainAxisSpacing: 12,
              childAspectRatio: 1,
            ),
            itemCount: 10,
            itemBuilder:
                (context, index) => GestureDetector(
                  onTap: () {
                    context.push('/main/digital-business-cards/$index');
                  },
                  child: Container(
                    decoration: BoxDecoration(
                      borderRadius: BorderRadius.circular(24),
                      gradient: LinearGradient(
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                        colors: [
                          themeProvider.themeMode == ThemeMode.dark
                              ? Color.fromARGB(255, 40, 40, 40)
                              : Color.fromARGB(255, 235, 235, 235),
                          themeProvider.themeMode == ThemeMode.dark
                              ? Color.fromARGB(255, 30, 30, 30)
                              : Color.fromARGB(255, 245, 245, 245),
                          themeProvider.themeMode == ThemeMode.dark
                              ? Color.fromARGB(255, 20, 20, 20)
                              : Color.fromARGB(255, 255, 255, 255),
                        ],
                      ),
                    ),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          Icons.card_giftcard,
                          size: 64,
                          color:
                              themeProvider.themeMode == ThemeMode.dark
                                  ? Colors.white
                                  : Colors.black87,
                        ),
                        SizedBox(height: 8),
                        Text(
                          'Kartvizit',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        Text(
                          '#${index + 1}',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
          ),
        ),
      ],
    );
  }
}
