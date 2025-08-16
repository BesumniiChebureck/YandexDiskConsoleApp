# Yandex.Disk Console Client

## 📝 Описание проекта

Консольное приложение на C# для работы с Яндекс.Диском с помощью NuGet пакета YandexDisk.Client. Позволяет просматривать содержимое диска, создавать папки, загружать и скачивать файлы прямо из командной строки.

## 🌟 Основные возможности

- 🔍 Просмотр содержимого директорий на Яндекс.Диске
- 📁 Создание новых папок
- 🔄 Загрузка файлов на диск
- ⬇️ Скачивание файлов (и получение ссылки на скачивание файла)

## 🛠 Технологии

- .NET Framework 4.7.2 (код работы с библиотекой YandexDisk.Client будет аналогичен для .NET Core)
- [YandexDisk.Client](https://github.com/raidenyn/yandexdisk.client) - официальная .NET библиотека для работы с Яндекс.Диском
- Dependency Injection
- Асинхронное программирование

## ⚙️ Настройка

1. Получите OAuth-токен Яндекс.Диска:
   - Зарегистрируйте приложение на [Яндекс.OAuth](https://oauth.yandex.ru/) (в Redirect URI для веб-сервисов можно указать любой адрес, например http://localhost:12345/callback)
   - Получите токен для своего аккаунта:
      - Выполните запрос https://oauth.yandex.ru/authorize?response_type=token&client_id=<Ваш_clientId>
      - Получите ответ http://localhost:12345/callback#access_token=<Ваш_OAuth_token>&token_type=bearer&expires_in=<Время_жизни_токена>
      - Скопируйте значение из access_token

2. Настройте приложение:
   - Создайте файл `appsettings.json` в папке с программой (/YandexDiskApp/appsettings.json):
     ```json
     {
       "YandexDisk": {
         "AccessToken": "ваш_OAuth_токен_здесь"
       }
     }
     ```

## 🚀 Запуск

```bash
dotnet run
```

## 📋 Примеры использования

1. Просмотр корневой директории:
   ```
   > view /
   ```

2. Создание папки:
   ```
   > mkdir /TestFolder
   ```

3. Загрузка файла:
   ```
   > upload path_to_file path_on_disk
   ```
   ```
   > upload C:\example\img1.png /TestFolder
   ```

4. Скачивание файла (параметр path_to_destination_on_computer необязателен):
   ```
   > download path_to_file_on_disk path_to_destination_on_computer
   ```
   ```
   > download /TestFolder/test1.png C:\example\
   ```

5. Скачивание файла:
   ```
   > download-link path_to_file_on_disk
   ```
   ```
   > download-link /TestFolder/test1.png
   ```

### Варианты использования
1. Можно собрать проект и добавить в переменные окружения, после чего использовать в консоли следующим образом:
```
YandexDiskApp view /
YandexDiskApp mkdir /TestFolder
```
2. Просто запустить проект как консольное приложение и использовать в интерактивном режиме:
```
> view /
> mkdir /TestFolder
> exit
```

## 📌 Особенности реализации

- Чистая архитектура с разделением на слои
- Внедрение зависимостей через Microsoft DI
- Полностью асинхронный код
- Обработка ошибок API
