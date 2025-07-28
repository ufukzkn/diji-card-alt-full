import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:stajproje/screens/main/main_screen.dart';
import 'package:stajproje/screens/main/my_business_cards.dart';
import 'package:stajproje/screens/main/scan_screen.dart';
import 'package:stajproje/screens/main/settings_screen.dart';
import 'package:stajproje/widgets/fade_in_wrapper.dart';
import 'package:stajproje/l10n/app_localizations.dart';

class MainLayout extends StatefulWidget {
  const MainLayout({super.key});

  @override
  State<MainLayout> createState() => _MainLayoutState();
}

final headers = [
  const FHeader(title: Text('Anasayfa')),
  const FHeader(title: Text('Kartvizitlerim')),
  const FHeader(title: Text('Tara')),
  const FHeader(title: Text('Profil')),
];

final contents = [
  const FadeInWrapper(child: MainScreen()),
  const FadeInWrapper(child: MyBusinessCards()),
  const FadeInWrapper(child: ScanScreen()),
  const FadeInWrapper(child: SettingsScreen()),
];

class _MainLayoutState extends State<MainLayout> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context)!;
    return FScaffold(
      footer: FBottomNavigationBar(
        index: _index,
        onChange: (index) => setState(() => _index = index),
        children: [
          FBottomNavigationBarItem(
            icon: Icon(FIcons.house),
            label: Text(l10n.home, style: context.theme.typography.xs),
          ),
          FBottomNavigationBarItem(
            icon: Icon(FIcons.layoutGrid),
            label: Text(l10n.myCards, style: context.theme.typography.xs),
          ),
          FBottomNavigationBarItem(
            icon: Icon(FIcons.camera),
            label: Text(l10n.scan, style: context.theme.typography.xs),
          ),
          FBottomNavigationBarItem(
            icon: Icon(FIcons.settings),
            label: Text(l10n.settings, style: context.theme.typography.xs),
          ),
        ],
      ),
      //header: headers[_index],
      child: contents[_index],
    );
  }
}
