import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:forui/forui.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';

import 'package:stajproje/widgets/profile_sheet.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _nameController = TextEditingController();
  final TextEditingController _surnameController = TextEditingController();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _jobController = TextEditingController();
  final TextEditingController _companyController = TextEditingController();

  File? _selectedImage;
  bool _isPickingImage = false;
  final List<Map<String, dynamic>> _profileInfo = [];

  void _addProfileInfo(Icon icon, String title, String content) {
    setState(() {
      _profileInfo.add({'icon': icon, 'title': title, 'content': content});
    });
  }

  void _removeProfileInfo(int index) {
    setState(() {
      _profileInfo.removeAt(index);
    });
  }

  Future<void> _pickImage() async {
    if (_isPickingImage) return;

    setState(() {
      _isPickingImage = true;
    });

    try {
      final picker = ImagePicker();
      final pickedFile = await picker.pickImage(source: ImageSource.gallery);

      if (pickedFile != null) {
        setState(() {
          _selectedImage = File(pickedFile.path);
        });
      }
    } catch (e) {
      debugPrint('Resim seçilirken hata oluştu: $e');
    } finally {
      setState(() {
        _isPickingImage = false;
      });
    }
  }

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _nameController.dispose();
    _surnameController.dispose();
    _emailController.dispose();
    _jobController.dispose();
    _companyController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FScaffold(
      header: FHeader.nested(
        title: Text('Profili Düzenle'),
        prefixes: [
          FHeaderAction.back(
            onPress: () {
              context.go('/main/home');
            },
          ),
        ],
      ),
      child: SingleChildScrollView(
        child: FCard(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Text(
                  '* Bu bilgiler kartvizitlerinizde görünecektir.',
                  style: TextStyle(fontSize: 14),
                ),
                const SizedBox(height: 10),
                Stack(
                  alignment: Alignment.center,
                  children: [
                    ClipRRect(
                      borderRadius: BorderRadius.circular(100),
                      child:
                          _selectedImage != null
                              ? Image.file(
                                _selectedImage!,
                                width: 150,
                                height: 150,
                                fit: BoxFit.cover,
                              )
                              : Image.network(
                                "https://media.istockphoto.com/id/1682296067/photo/happy-studio-portrait-or-professional-man-real-estate-agent-or-asian-businessman-smile-for.jpg?s=612x612&w=0&k=20&c=9zbG2-9fl741fbTWw5fNgcEEe4ll-JegrGlQQ6m54rg=",
                                width: 150,
                                height: 150,
                                fit: BoxFit.cover,
                              ),
                    ),
                    Positioned(
                      top: 0,
                      right: 0,
                      child: GestureDetector(
                        onTap: _pickImage,
                        child: Container(
                          width: 30,
                          height: 30,
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            color: Colors.white,
                            border: Border.all(color: Colors.grey.shade300),
                          ),
                          child: const Icon(Icons.edit, size: 18),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                FTextFormField(
                  label: const Text('Ad'),
                  controller: _nameController,
                  hint: 'John',
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  validator:
                      (value) =>
                          3 <= (value?.length ?? 0)
                              ? null
                              : 'Ad en az 3 karakterden oluşmalıdır.',
                ),
                const SizedBox(height: 10),
                FTextFormField(
                  label: const Text('Soyad'),
                  controller: _surnameController,
                  hint: 'Doe',
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  validator:
                      (value) =>
                          3 <= (value?.length ?? 0)
                              ? null
                              : 'Soyad en az 3 karakterden oluşmalıdır.',
                ),
                const SizedBox(height: 10),
                FTextFormField.email(
                  label: const Text('E-posta'),
                  controller: _emailController,
                  hint: 'john@doe.com',
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  validator:
                      (value) =>
                          (value?.contains('@') ?? false)
                              ? null
                              : 'Lütfen geçerli bir e-posta adresi giriniz.',
                ),
                const SizedBox(height: 10),
                FTextFormField(
                  label: const Text('Şirket'),
                  controller: _companyController,
                  hint: 'OctaDigital',
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  validator:
                      (value) =>
                          3 <= (value?.length ?? 0)
                              ? null
                              : 'Ünvan en az 3 karakterden oluşmalıdır.',
                ),
                const SizedBox(height: 10),
                FTextFormField(
                  label: const Text('Ünvan'),
                  controller: _jobController,
                  hint: 'Flutter Developer',
                  autovalidateMode: AutovalidateMode.onUserInteraction,
                  validator:
                      (value) =>
                          3 <= (value?.length ?? 0)
                              ? null
                              : 'Ünvan en az 3 karakterden oluşmalıdır.',
                ),
                const SizedBox(height: 10),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Bilgiler',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    FButton(
                      style: FButtonStyle.ghost(),
                      onPress:
                          () => showFSheet(
                            context: context,
                            side: FLayout.btt,
                            builder:
                                (context) => ProfileSheet(
                                  side: FLayout.btt,
                                  onAddProfileInfo: _addProfileInfo,
                                ),
                          ),
                      child: const Icon(FIcons.listPlus),
                    ),
                  ],
                ),
                const SizedBox(height: 10),
                // Profil bilgilerini gösterme bölümü
                if (_profileInfo.isNotEmpty)
                  ListView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    padding: EdgeInsets.zero,
                    itemCount: _profileInfo.length,
                    itemBuilder: (context, index) {
                      final info = _profileInfo[index];
                      return Column(
                        children: [
                          FTile(
                            prefix: info['icon'],
                            title: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  info['title'],
                                  style: TextStyle(fontWeight: FontWeight.w600),
                                ),
                                Text(info['content']),
                              ],
                            ),
                            suffix: GestureDetector(
                              onTap: () {
                                _removeProfileInfo(index);
                              },
                              child: FBadge(
                                style: FBadgeStyle.destructive(),
                                child: const Text('Sil'),
                              ),
                            ),
                          ),
                          SizedBox(height: 10),
                        ],
                      );
                    },
                  ),
                if (_profileInfo.isEmpty)
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      border: Border.all(color: Colors.grey.shade300),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: const Text(
                      'Henüz bilgi eklenmemiş.',
                      style: TextStyle(
                        color: Colors.grey,
                        fontStyle: FontStyle.italic,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ),
                const SizedBox(height: 20),
                FButton(
                  child: const Text('Güncelle'),
                  onPress: () {
                    if (!_formKey.currentState!.validate()) {
                      return; // Form is invalid.
                    }

                    // Form is valid, do something.
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
