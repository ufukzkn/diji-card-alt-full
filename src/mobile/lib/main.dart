import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'package:forui/forui.dart';
import 'package:stajproje/providers/theme_provider.dart';
import 'package:stajproje/screens/forgot_password.dart';
import 'package:stajproje/layouts/main_layout.dart';
import 'package:stajproje/screens/home_screen.dart';
import 'package:stajproje/screens/main/digital_business_card.dart';
import 'package:stajproje/screens/main/profile_screen.dart';
import 'package:stajproje/screens/user_agreement_page.dart';
import 'package:stajproje/screens/auth_screen.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:stajproje/l10n/app_localizations.dart';
import 'package:stajproje/providers/locale_provider.dart';

final _router = GoRouter(
  initialLocation: '/',
  routes: [
    GoRoute(path: '/', builder: (context, state) => const HomeScreen()),
    GoRoute(path: '/auth', builder: (context, state) => const AuthScreen()),
    GoRoute(
      path: '/user-agreement',
      builder: (context, state) => const UserAgreementPage(),
    ),
    GoRoute(
      path: '/forgot-password',
      builder: (context, state) => const ForgotPasswordPage(),
    ),
    GoRoute(
      path: '/main/home',
      builder: (context, state) => const MainLayout(),
    ),
    GoRoute(
      path: '/main/digital-business-cards/:digitalBusinessCardId',
      builder:
          (context, state) => DigitalBusinessCard(
            digitalBusinessCardId: int.parse(
              state.pathParameters['digitalBusinessCardId'] ?? '0',
            ),
          ),
    ),
    GoRoute(
      path: '/main/profile',
      builder: (context, state) => const ProfileScreen(),
    ),
  ],
);

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final themeProvider = ThemeProvider();
  await themeProvider.loadThemeMode();

  final localeProvider = LocaleProvider();
  await localeProvider.loadLocale();

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider.value(value: themeProvider),
        ChangeNotifierProvider.value(value: localeProvider),
      ],
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer2<ThemeProvider, LocaleProvider>(
      builder: (context, themeProvider, localeProvider, child) {
        final isDarkMode = themeProvider.themeMode == ThemeMode.dark;

        return MaterialApp.router(
          routerConfig: _router,
          debugShowCheckedModeBanner: false,
          locale: localeProvider.locale,
          supportedLocales: LocaleProvider.supportedLocales,
          localizationsDelegates: const [
            AppLocalizations.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
            GlobalCupertinoLocalizations.delegate,
          ],
          builder:
              (context, child) => FTheme(
                data: FThemeData(
                  colors:
                      isDarkMode
                          ? FThemes.zinc.dark.colors
                          : FThemes.zinc.light.colors,
                  typography: FTypography(defaultFontFamily: 'Montserrat'),
                  textFieldStyle: FTextFieldStyle(
                    labelPadding: EdgeInsets.only(bottom: 5),
                    keyboardAppearance:
                        FTheme.of(context).textFieldStyle.keyboardAppearance,
                    clearButtonStyle:
                        FTheme.of(context).textFieldStyle.clearButtonStyle,
                    contentTextStyle: FWidgetStateMap.all(
                      FTheme.of(context).textFieldStyle.contentTextStyle
                          .resolve({})
                          .copyWith(
                            fontFamily: 'Montserrat',
                            color:
                                isDarkMode
                                    ? context.theme.colors.primaryForeground
                                    : context.theme.colors.secondaryForeground,
                          ),
                    ),
                    hintTextStyle: FWidgetStateMap.all(
                      FTheme.of(context).textFieldStyle.hintTextStyle
                          .resolve({})
                          .copyWith(
                            fontFamily: 'Montserrat',
                            color:
                                isDarkMode
                                    ? context.theme.colors.primaryForeground
                                    : context.theme.colors.secondaryForeground,
                          ),
                    ),
                    counterTextStyle: FWidgetStateMap.all(
                      FTheme.of(context).textFieldStyle.counterTextStyle
                          .resolve({})
                          .copyWith(
                            fontFamily: 'Montserrat',
                            color:
                                isDarkMode
                                    ? context.theme.colors.primaryForeground
                                    : context.theme.colors.secondaryForeground,
                          ),
                    ),
                    border: FTheme.of(context).textFieldStyle.border,
                    labelTextStyle: FWidgetStateMap.all(
                      FTheme.of(context).textFieldStyle.labelTextStyle
                          .resolve({})
                          .copyWith(
                            fontFamily: 'Montserrat',
                            color:
                                isDarkMode
                                    ? context.theme.colors.primaryForeground
                                    : context.theme.colors.secondaryForeground,
                          ),
                    ),
                    descriptionTextStyle: FWidgetStateMap.all(
                      FTheme.of(context).textFieldStyle.descriptionTextStyle
                          .resolve({})
                          .copyWith(
                            fontFamily: 'Montserrat',
                            color:
                                isDarkMode
                                    ? context.theme.colors.primaryForeground
                                    : context.theme.colors.secondaryForeground,
                          ),
                    ),
                    errorPadding: EdgeInsets.only(top: 5),
                    errorTextStyle: FTheme.of(
                      context,
                    ).textFieldStyle.errorTextStyle.copyWith(
                      fontFamily: 'Montserrat',
                      color: context.theme.colors.error,
                    ),
                  ),
                ),
                child: child!,
              ),
        );
      },
    );
  }
}
