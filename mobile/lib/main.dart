import 'package:flutter/material.dart';
import 'app.dart';
import 'services/strings.dart';

void main() async {
	WidgetsFlutterBinding.ensureInitialized();
	await Strings.load('zh_CN');
	runApp(const TheRpgApp());
}


