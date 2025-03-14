This is a detailed version of the steam workshop description (desc.txt).

# About

This mod fixes some bugs in HatSingularity (1.0.0.9) and also fixes the lack of Japanese support in the latest version.
HatSingularity itself is not included, so you will need to subscribe to it separately.

# Fixed bugs

- If you change the language, translation will not work properly until you restart the game. (Temporary fix, may break depending on mods that depend on the hat)
- Missing translations for "shield" in en and "Critical Hit" in kr.
- The last three keywords in the Battle Page Filter are blank in all languages.
- The "Singleton" icon in the filter is not displayed.
- The selected hand page is not brought to the foreground.
- Many issues with the Multiple preview system during battle.
  - Details in Japanese only

# Other changes

- `[抵抗]` (Resilience) is no longer replaced in jp. (Because it's name is the same as Resist's "endured" in JP)
- The Multiple preview system during battle has been almost remade.
  - No matter which preview page you move the cursor over, the first one will be displayed in detail.
  - To select, use the mouse wheel or left and right keys as before.

---

# 概要

HatSingularity(1.0.0.9)に含まれているいくつかのバグを修正し、最新バージョンの日本語対応を追加します。
日本語で未訳だったキーワード語は適当に訳しました。
HatSingularity自体は含まれてないので別途サブスクライブが必要です。

# 修正したバグ

- 言語を変更すると、ゲームを再起動するまで翻訳が正常に動作しません。(暫定的な修正、Hatに依存するMod次第で壊れるかもしれません)
- enのshield、krのCritical Hitの翻訳が欠落しています。
- バトルページフィルターの最後の3つのキーワードが空欄になっています。
- フィルターの唯一のアイコンが表示されていません。
- 手札の選択中ページが最前面に表示されません。
- 戦闘中のマルチプレビューシステムの数多くの問題。
  - 一度プレビューページにカーソルを合わせると、ページを選択するまで、手札表示中常にホイール操作が有効なままになってしまいます。
  - ホイール操作に全てのページのプレビューが反応してしまいます。
  - プレビューがクリックで選択できてしまいます。
  - ページ選択後、速度スロット選択中もページを選びなおすとプレビューが表示されます。
  - プレビュー表示中に右クリックで手札リスト自体を閉じると、次開いたときにプレビューが表示されたままです。
  - マウスで選択中のページと、ホイールで選択中のページが連動しません。
  - 初期化に二次関数的に時間がかかり、6枚くらい表示すると一瞬画面が固まります。
  - PreviewCard.xmlの「素敵な生地」を追加するページ(素敵な糸が出ます)のIDが間違っています。

# その他の変更

- jpでは`[抵抗]`は置換されなくなりました。(状態異常と耐性で同じ名前になっているため)
- 戦闘中のマルチプレビューシステムはほぼ作り直されました。
  - どのプレビューページにカーソルを移動させても、1枚目が詳細表示されます。
  - 選択するには今までのようにマウスホイールか左右キーを使用します。

---
# others

GitHub Link: https://github.com/kaz-mighty/LoRMods

Credit
HatSingularity (https://steamcommunity.com/sharedfiles/filedetails/?id=3051354156)
