using UnityEngine;
using System.Collections;

///
/// !!! Machine generated code !!!
/// !!! DO NOT CHANGE Tabs to Spaces !!!
/// 
[System.Serializable]
public class SkillDataData
{
  [SerializeField]
  int id;
  public int ID { get {return id; } set { this.id = value;} }
  
  [SerializeField]
  string skillname;
  public string Skillname { get {return skillname; } set { this.skillname = value;} }
  
  [SerializeField]
  string skillexplanation;
  public string Skillexplanation { get {return skillexplanation; } set { this.skillexplanation = value;} }
  
  [SerializeField]
  SkillType skilltype;
  public SkillType SKILLTYPE { get {return skilltype; } set { this.skilltype = value;} }
  
  [SerializeField]
  SkillCategory skillcategory;
  public SkillCategory SKILLCATEGORY { get {return skillcategory; } set { this.skillcategory = value;} }
  
  [SerializeField]
  float skillrange;
  public float Skillrange { get {return skillrange; } set { this.skillrange = value;} }
  
  [SerializeField]
  float cooldown;
  public float Cooldown { get {return cooldown; } set { this.cooldown = value;} }
  
}