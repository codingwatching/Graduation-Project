using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ObserverPattern;

public class StatusPanel : PanelUIComponent
{
    [SerializeField] public TextMeshProUGUI unitName;
    [SerializeField] public TextMeshProUGUI unitDescription;
    [SerializeField] public TextMeshProUGUI unitDamage;
    [SerializeField] public Image unitcurrHealth;
    [SerializeField] public Image unitImage;
    [SerializeField] public Image unitSkill;

    public override void SetPanel(UIParam u)
    {
        if (u == null) return;

        UIStatusParam us = (UIStatusParam)u;
        Unit unit = us._u;

        gameObject.SetActive(true);

        unitName.text = unit.name;
        unitDescription.text = "Description For Unit.";
        unitDamage.text = $"{unit.basicAttackDamageMin.ToString()} ~ {unit.basicAttackDamageMax.ToString()}";
        unitcurrHealth.fillAmount = unit.currHealth / (float)unit.maxHealth;
        unitImage.sprite = unit.icon;

        // -> skill은 아직 이미지가 없어서~
    }

    public override void UnsetPanel()
    {
        gameObject.SetActive(false);
    }
}
