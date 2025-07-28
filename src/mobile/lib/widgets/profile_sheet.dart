import 'package:flutter/material.dart';
import 'package:forui/forui.dart';
import 'package:flutter_iconpicker/flutter_iconpicker.dart';

class ProfileSheet extends StatefulWidget {
  final FLayout side;
  final Function(Icon, String, String) onAddProfileInfo;

  const ProfileSheet({
    required this.side,
    required this.onAddProfileInfo,
    super.key,
  });

  @override
  State<ProfileSheet> createState() => _ProfileSheetState();
}

class _ProfileSheetState extends State<ProfileSheet>
    with SingleTickerProviderStateMixin {
  late final _titleController = FSelectController<String>(vsync: this);
  final TextEditingController _contentController = TextEditingController();

  final _formKey = GlobalKey<FormState>();

  IconPickerIcon? _icon;

  _pickIcon() async {
    IconPickerIcon? icon = await showIconPicker(context);

    if (icon != null) {
      setState(() {
        _icon = icon;
      });
    }

    debugPrint('Picked Icon:  $icon');
  }

  static const _titles = [
    'Website',
    'Github',
    'Linkedin',
    'Instagram',
    'Facebook',
    'Twitter',
    'Youtube',
    'Tiktok',
    'Discord',
  ];

  @override
  Widget build(BuildContext context) => Container(
    height: double.infinity,
    width: double.infinity,
    decoration: BoxDecoration(
      color: context.theme.colors.background,
      border:
          widget.side.vertical
              ? Border.symmetric(
                horizontal: BorderSide(color: context.theme.colors.border),
              )
              : Border.symmetric(
                vertical: BorderSide(color: context.theme.colors.border),
              ),
    ),
    child: Padding(
      padding: const EdgeInsets.symmetric(horizontal: 10.0, vertical: 8.0),
      child: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Bilgi Ekle',
              style: context.theme.typography.xl2.copyWith(
                fontWeight: FontWeight.w600,
                color: context.theme.colors.foreground,
                height: 1.5,
              ),
            ),
            Text(
              'Kartvizitlerinizde görünecek bilgileri ekleyebilirsiniz.',
              style: context.theme.typography.sm.copyWith(
                color: context.theme.colors.mutedForeground,
              ),
            ),
            const SizedBox(height: 8),
            SizedBox(
              width: 450,
              child: Form(
                key: _formKey,
                child: Column(
                  children: [
                    FButton(
                      style: FButtonStyle.outline(),
                      onPress: _pickIcon,
                      child:
                          _icon != null
                              ? Icon(_icon!.data)
                              : const Text('İkon seçiniz'),
                    ),
                    const SizedBox(height: 10),
                    FSelect<String>(
                      controller: _titleController,
                      label: const Text('Alan adı'),
                      hint: 'Alan seçiniz',
                      format: (s) => s,
                      children: [
                        for (final title in _titles) FSelectItem(title, title),
                      ],
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'Alan adı boş olamaz';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 10),
                    FTextFormField(
                      label: Text('İçerik'),
                      controller: _contentController,
                      hint: 'Örnek: www.yoursite.com',
                      validator: (value) {
                        if (value == null || value.isEmpty) {
                          return 'İçerik boş olamaz';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    FButton(
                      onPress: () {
                        if (_formKey.currentState!.validate() &&
                            _icon != null) {
                          widget.onAddProfileInfo(
                            Icon(_icon!.data),
                            _titleController.value.toString(),
                            _contentController.text.toString(),
                          );
                          Navigator.of(context).pop();
                        } else {
                          debugPrint("Lütfen tüm alanları doldurunuz.");
                        }
                      },
                      child: const Text('Ekle'),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    ),
  );
}
