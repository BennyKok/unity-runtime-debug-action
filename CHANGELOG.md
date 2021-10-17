# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [1.1.0](https://gitlab.com/BennyKok/unity-runtime-debug-action/compare/v1.0.1...v1.1.0) (2021-01-06)


### Features

* make sure the default settings is not editable with extra GUI ([d1906c7](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/d1906c7cc762702f2d3ced593b6419be1290ca8c))
* UnityEvent bindings for Flag component ([671c0f0](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/671c0f02b9aa8d522c5d2035c88a53e57b276609))


### Bug Fixes

* action display should not be interactable ([6d00109](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/6d0010935295e8767db8e11fb74af9cbb343cde5))
* closePanelAfterTrigger not working with toggle action ([700fa94](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/700fa94da2e3f313c1c10e55b5330541f7957474))
* component action not setup property upon registering ([21ad00d](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/21ad00de03167cc298520dc30226f0768112e04b))
* menu display error when reload scene sometimes ([7d59973](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/7d59973410ca91574cb772ac7ccd0e7d33b43481))
* rename IsIsSystemEnabled -> IsSystemEnabled ([630188c](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/630188ccb253bd623de71bb900dcd3b390c22720))
* **editor:** component action compatibility with Odin ([d285539](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/d2855390fe06254886079cc0d23bd705b0257886))

## 1.0.1 (2020-12-26)

### Refactor
*  Extract world space UI prefab from VR integration

## 1.0.0 (2020-12-14)


### Features

* 🎸 folder support for action, init back navigation support ([4bb6211](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/4bb62111b3a2606ccd654355094c0141084c54d7))
* 🎸 init search impl, fix remove action not removing view ([cb82e86](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/cb82e8651f7fdaaf901661a03c3044787bba3f0f))
* 🎸 keyboard navigation for action menu ([7972213](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/797221338eb6f10c1cb47c49d9196437a07d2c86))
* 🎸 simple action search implemented ([63797e6](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/63797e62629ee7441d222ab425bfd9ff0afbae0b))
* 🎸 swap theme action and settings ([28d92e7](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/28d92e7265c155aef07e9259b9877248ed6d7235))
* add enable mode in settings ([2c4826a](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/2c4826a778ef6c4acc46b0c32946a87ddb8c543b))
* basic reflection test ([fd384df](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/fd384df87b946aacce540c61303b98ac0ab727ef))
* custom font in DebugUIHandler ([13e3ab0](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/13e3ab00f53014d7a41318813795fbcabae84554))
* init dashsu demo ([89eafbb](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/89eafbbf36e33fbe00a966b4c3b21de8f3074bb8))
* init input system support ([30a05bf](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/30a05bf222d1e46e295a9927e80f76c578b50d53))
* MenuPauseMode ([f747113](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/f74711370b392aa794768399c39e6325104494da))
* new compact bottom theme ([b3626a7](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/b3626a7741198c482ff7b0eed94d8ffeef63de40))
* quality settings actions now dynamically populated ([335b405](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/335b405787f16adff7cc09cccb7ad59025d99292))
* reflection support input, flag ([803cd44](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/803cd44b049eaf8b3caec41dcb89e5c192629f98))
* search by tag, remove by path ignore tag ([76cbb96](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/76cbb96e42ccce66e271ab737c20c9cacfadcc19))
* some api ([5eb6469](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/5eb646951278b490e7c87707c96fdcc19234d806))
* **demo:** reveal text demo, set text action ([2ce3295](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/2ce3295aa373277a1ebd055b7f14b778634f15c2))
* theme settings will now be saved ([038172d](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/038172d7dffdfdbeaf8dee90d54f7c775f816f82))


### Bug Fixes

* 🐛 minor tweak ([47c9930](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/47c99304b3cf4abcca000b1ef78c49892e781de4))
* 🐛 use AssetPostprocessor for settings file check at start ([ac7a22e](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/ac7a22ef8c81179c178d1f64665d79318e26c098))
* crash error because ui is null in ListItemView ([8471b71](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/8471b71996f5b4c7c68d86d7a41aa12ef8a86f91))
* first frame flicker list selection ([8bc7f44](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/8bc7f4491174975a07db78190b4c72f98c4c517b))
* shortcut action triggered when during search ([9b51b5e](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/9b51b5e2e5233b032abfb95c0263a3d7cffa8ac8))
* tag display ([c932dd4](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/c932dd4f0b9eaaf01025cb4280e374fe1f8ed275))
* time not paused when switch theme ([167b303](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/167b3036e09ad9c033aa1f059c17c3a0b650eda3))
* tmp late import screw up text display ([d753ea3](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/d753ea37620c78e1c829ada9ef371bbd1bb6ba25))
* uiParent null in sub list ([3458e8f](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/3458e8f9bd376bb3552139c256937e000e448f22))
* UnregisterActionsAuto ([fd41ec8](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/fd41ec87dde218bf930bc8bfa5c56a1dcf3f2d00))
* visibility attribute to clean up RDASettings ([c1c6d7c](https://gitlab.com/BennyKok/unity-runtime-debug-action/commit/c1c6d7cfc4b7cfc9cbcc6c26659334a260f86031))