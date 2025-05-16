# 🧠 Dynamic Input System for Educational Geometry Shapes (Unity)

## 📁 Prefab Setup Instructions

### 1. Create `InputFieldRow.prefab`
- Structure:
```
InputFieldRow (GameObject)
├── Label (TextMeshProUGUI)
└── Input (TMP_InputField)
```
- Attach TextMeshProUGUI to `Label`, TMP_InputField to `Input`

### 2. Setup `DynamicInputPanel`
- Create an empty GameObject called `DynamicInputPanel`
- Add component: `DynamicInputPanel.cs`
- Add a child `Container` (with Vertical Layout Group)
- Create Submit Button with label "Vẽ hình" and set `OnClick` → `ShapeInputController.OnSubmit()`

### 3. Setup `ShapeInputController`
- Add component `ShapeInputController.cs` to a controller object
- Assign `inputPanel` reference to the `DynamicInputPanel` object

---

## 🧪 Runtime Usage

In any MonoBehaviour (e.g., startup handler):
```csharp
shapeInputController.SetSpawner(new SquareSpawner());
```

---

## 📦 Required Scripts

- FieldDefinition.cs
- FieldType.cs
- ShapeSolver.cs
- DynamicInputPanel.cs
- ShapeInputController.cs

Make sure to include all above in your Unity `Scripts/Manipulator` folder.
