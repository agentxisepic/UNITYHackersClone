using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IImplementUI
{
    string Name { get; set; }
    int Level { get; set; }

    void RequestUI();

    void UpdateUI();

    void Upgrade();

}
