# Служба управления складскими запасами на gRPC

## Обзор
  Проект реализует систему управления складскими остатками с использованием gRPC-сервиса на платформе .NET. Поддерживается добавление и удаление товаров,
обновление остатков, просмотр списка товаров и получение информации о конкретной позиции. 

![image](https://github.com/user-attachments/assets/49347c2f-9b99-4f8e-9e8d-f023d149a130)

Реализованы все методы и сущности описанные в задании №14

![image](https://github.com/user-attachments/assets/2c0926f2-8068-499e-863f-5cf90da9e678)

![image](https://github.com/user-attachments/assets/cd91d71e-f2e4-4e94-88c8-5bfd645e7ba3)


## Сценарий
Сервис позволяет клиентским приложениям управлять товарами на складе в реальном времени. Пользователь может добавлять товары, изменять количество на складе, получать список всех товаров и получать информацию по каждому товару.

## Требования
- Visual Studio 2019 или 2022
- .NET 7.0 или выше
- gRPC для .NET: Grpc.AspNetCore, Grpc.Tools

## Установка
Скачиваем проект сдесь и запускаем файл InventoryService.sln в VisualStudio
Проверьте зависимости:
В Visual studio в файлах InventoryGrpcClient.csproj, InventoryService.csproj необходимы следущие версии Grpc
```bash
    <PackageReference Include="Grpc.Net.Client" Version="2.55.0" />
    <PackageReference Include="Google.Protobuf" Version="3.23.3" />
    <PackageReference Include="Grpc.Tools" Version="2.55.0" PrivateAssets="All" />
```
inventory.proto включен в проекте с настройкой:
```bash
    <Protobuf Include="Protos\inventory.proto" GrpcServices="Server" />
```
## Запуск службы
1. Запустите проект сервера InventoryService через Visual Studio

2. Запустите проект клиента InventoryGrpcClient через Visual Studio

## Пример использования
Клиент имеет следущее меню для действий

![image](https://github.com/user-attachments/assets/49347c2f-9b99-4f8e-9e8d-f023d149a130)

1. Вводим в консоль `1` и сервис выводит все блюда

![image](https://github.com/user-attachments/assets/85b636d9-a46a-4993-94e3-9c977fd4aaff)

2. Вводим в консоль `2`, придумываем название и количество продуктов (например `food` `150`).
    
![image](https://github.com/user-attachments/assets/f7216293-1623-4428-b0f8-db59172c23b3)

Происходит добавление нового продукта.

3. Вводим в консоль `3`, вводим ID нужного продукта (например `1`).

![image](https://github.com/user-attachments/assets/b0e10125-ff78-46c5-a6b0-79582e7d535c)

происходит вывод данных по ID выбранного блюда.

4. Вводим в консоль `4`, вводим ID и придумываем новую разницу блюда (например `1`, `150`)

![image](https://github.com/user-attachments/assets/03350390-5017-45d6-bc3c-dd642a42474b)

К выбранному блюду добавляется или вычитается если добавить `-` данная сумма.

5. Вводим в консоль `5`, придумываем ID блюда для удаления (например `6`).

![image](https://github.com/user-attachments/assets/86d43170-f18f-4161-a631-ddef885fdbf5)
Происходит удаление данного блюда

6. Вводим в консоль `6`, происходит отключение пользователя.

![image](https://github.com/user-attachments/assets/9977bee8-3fef-4e6f-8fc5-7fad86aabe8e)

## Тестирование службы
1. **Тест сохранения данных**:
   - Проверьте `products.json` после изменения данных, например приведенных выше в `Сценариях`.
   - Перезапустите сервер и присоединитесь в качестве пользователя для проверки сохранения данных.

2.  **Обработка исключений**:
     Ситуациями с исключениями в данном проекте являются просмотр, добавление, удаление несуществующих продуктов (по их `ID`), в проекте предусмотрены все возможные исключения и вывод пользователю информации об этих ошибках.

![image](https://github.com/user-attachments/assets/1797b88c-81f9-44e0-bbe3-c94e7d4577a7)

     
## Дополнительные возможности
- **Сохранение данных**: Файл products.json для хранения запасов.
Структура данных в файле:

![image](https://github.com/user-attachments/assets/0a1624c3-7ad1-4d59-ba9a-b52630e6623e)
