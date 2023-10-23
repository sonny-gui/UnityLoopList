using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct LoopData
{
    //可扩充。这里的属性随便改。根据需要的数据添加。
    public string name;
}

public class LoopItem
{
    public GameObject gameObject;//不能删除，核心代码必用
    public RectTransform rectTransform;//不能删除，核心代码必用
    public int dataIndex;//不能删除，核心代码必用
    public bool isWork;//不能删除，核心代码必用

    public Text name;//可删除，和具体逻辑有关
    //这里可以添加自定义的属性
}

public class LoopListTool : MonoBehaviour, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform origItemTrans;

    [HideInInspector]
    [SerializeField]
    public float space = 10;

    public Action<LoopItem, List<LoopData>, int> dataDeal;

    public List<LoopItem> itemPool = new List<LoopItem>();
    public List<LoopItem> showItems = new List<LoopItem>();

    [HideInInspector]
    [SerializeField]
    public float padding_left;

    [HideInInspector]
    [SerializeField]
    public float padding_right;

    [HideInInspector]
    [SerializeField]
    public float padding_top;

    [HideInInspector]
    [SerializeField]
    public float padding_bottom;

    private float lightBorder;
    private float rightBorder;
    private float topBorder;
    private float bottomBorder;

    private RectTransform rootTrans;
    private List<LoopData> data;
    private bool isDraging;
    private float dragingMove;
    [HideInInspector]
    [SerializeField]
    private bool _horizontalType;
    [HideInInspector]
    [SerializeField]
    private bool _verticalType;

    private bool moveLock = true;

    private float itemCellWidth;
    private float itemCellHeight;

    private float viewPortWidth;
    private float viewPortHeight;

    [HideInInspector]
    [SerializeField]
    private bool m_Inertia = true;
    public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

    [HideInInspector]
    [SerializeField]
    private bool m_isElastic = true;
    public bool isElastic { get { return m_isElastic; } set { m_isElastic = value; } }

    private float moveScale;
    private float elasticMove;
    private float elasticTarget;
    private float moveSpeed;
    private bool isCanTouchMove;
    /// <summary>
    /// item是否循环展示
    /// </summary>
    [HideInInspector]
    public bool isItemLoop = false;
    public bool horizontalType
    {
        get { return _horizontalType; }
        set
        {
            _horizontalType = value;
            _verticalType = !value;
        }
    }

    public bool verticalType
    {
        get { return _verticalType; }
        set
        {
            _verticalType = value;
            _horizontalType = !value;
        }
    }

    public void Awake()
    {

        //awake中为测试代码。LoopData这里生成临时数据
        List<LoopData> mData = new List<LoopData>();
        for (int i = 0; i < 9; i++)
        {
            LoopData loopData = new LoopData();
            loopData.name = i.ToString();
            mData.Add(loopData);
        }

        var isInit = this.MInit(mData, (mLoopItem, mLoopData, dataIndex) =>
        {
            mLoopItem.name.text = mLoopData[dataIndex].name;
        });

        if(isInit)
            this.InitItem();
    }

    public bool MInit(List<LoopData> _data, Action<LoopItem, List<LoopData>, int> _dataDeal)
    {
        this.rootTrans = this.gameObject.transform.GetComponent<RectTransform>();       
        if(this.origItemTrans == null )
            this.origItemTrans = this.rootTrans.Find("rawItem").GetComponent<RectTransform>();
        if (this.origItemTrans == null)
        {
            Debug.LogError("origItemTrans 错误 没有原始Item。请在root下一层 创建一个名字为rawItem的gameObject");
            return false;
        }               

        Mask m_mask = this.rootTrans.GetComponent<Mask>();
        if(m_mask == null)
        {
            Debug.LogError("rootTrans中，没有与脚本同级里mask。");
            return false;
        }
        if(m_mask && !m_mask.enabled )
        {
            Debug.LogError("rootTrans中，没有启用与脚本同级的mask组件。");
            return false;
        }

        Image m_image = this.rootTrans.GetComponent<Image>();
        if (m_image == null)
        {
            Debug.LogError("rootTrans中，没有与脚本同级里Image。");
            return false;
        }

        if (m_mask && !m_image.enabled)
        {
            Debug.LogError("rootTrans中，没有启用与脚本同级的Image组件。");
            return false;
        }

        if (_data == null)
        {
            Debug.LogError("没有初始化data数据");
            return false;
        }

        itemCellWidth = origItemTrans.rect.width;
        itemCellHeight = origItemTrans.rect.height;

        viewPortWidth = rootTrans.rect.width;
        viewPortHeight = rootTrans.rect.height;

        this.data = _data;

        isDraging = false;

        lightBorder = padding_left;
        rightBorder = this.viewPortWidth - padding_right;
        topBorder = -padding_top;
        bottomBorder = -this.viewPortHeight + padding_bottom;

        dataDeal = _dataDeal;

        if (!this.horizontalType && !this.verticalType)
            this.horizontalType = true;

        return true;
    }

    public void InitItem()
    {
        if (horizontalType)
        {
            float posX = this.padding_left;
            int nowDataIndex = 0;
            while (true)
            {
                if (posX > this.viewPortWidth)
                {
                    this.isCanTouchMove = true;
                    break;
                }                    
                if (nowDataIndex > this.data.Count - 1)
                {
                    if (this.isItemLoop)
                    {
                        nowDataIndex = 0;
                    }
                    else
                    {
                        this.isCanTouchMove = false;
                        break;
                    }
                }                  

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                newItem.rectTransform.anchoredPosition = new Vector2(posX, 0);
                this.showItems.Add(newItem);
                this.dataDeal(newItem, this.data, nowDataIndex);
                posX = posX + itemCellWidth + space;
                nowDataIndex++;
            }
        }
        else if (verticalType)
        {
            float posY = this.topBorder;
            int nowDataIndex = 0;
            while (true)
            {
                if (posY < -this.viewPortHeight)
                {
                    this.isCanTouchMove = true;
                    break;
                }

                if (nowDataIndex > this.data.Count - 1)
                {
                    if (this.isItemLoop)
                    {
                        nowDataIndex = 0;
                    }
                    else
                    {
                        this.isCanTouchMove = false;
                        break;
                    }
                }

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                newItem.rectTransform.anchoredPosition = new Vector2(0, posY);
                this.showItems.Add(newItem);
                this.dataDeal(newItem, this.data, nowDataIndex);
                posY = posY - itemCellHeight - space;
                nowDataIndex++;
            }
        }
    }

    public void ResetLayout()
    {
        for (int i = this.showItems.Count - 1; i >= 0; i--)
        {
            this.RecycleItem(this.showItems[i]);
            this.showItems.Remove(this.showItems[i]);
        }
        this.InitItem();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        this.isDraging = true;
        if (inertia)
            this.dragingMove = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.isDraging = true;
        if (this.horizontalType)
        {
            if (inertia)
                this.dragingMove = eventData.delta.x;
            this.LoopMove(eventData.delta.x);
        }
        else if(this.verticalType)
        {
            if (inertia)
                this.dragingMove = eventData.delta.y;
            this.LoopMove(eventData.delta.y);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDraging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.isDraging = true;
        this.dragingMove = 0;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void LateUpdate()
    {
        this.LoopMove_Inertia();
    }

    public void LoopMove(float moveDelta)
    {
        if (!isCanTouchMove)
            return;

        if(!this.moveLock)
            return;

        this.moveLock = false;

        if (this.horizontalType)
        {
            this.LoopMove_Horizontal(moveDelta);
        }
        else if(this.verticalType)
        {
            this.LoopMove_Vertical(moveDelta);
        }
    }

    public void LoopMove_Horizontal(float moveDelta)
    {
        if (!isItemLoop)//item不循环的时候，需要限制item是否在data的边界
        {
            if (moveDelta > 0)//向右滑动
            {
                //水平方向边界值处理
                if (isElastic)
                {
                    float lastItemX = this.showItems[0].rectTransform.anchoredPosition.x;
                    float lastData = this.showItems[0].dataIndex;
                    float minX = this.padding_left;
                    if (lastItemX > minX && lastData == 0)
                    {
                        moveScale = 1 - (lastItemX - minX) / (viewPortWidth - itemCellWidth/2);
                        moveDelta = moveDelta * moveScale;

                        this.elasticMove = lastItemX;
                        this.elasticTarget = minX;
                        this.dragingMove = 0;
                    }
                    else
                    {
                        this.elasticMove = 0;
                        this.moveScale = 0;
                    }
                }
                else
                {
                    float moveMax = this.padding_left - this.showItems[0].rectTransform.anchoredPosition.x + this.showItems[0].dataIndex * (itemCellWidth + space);
                    if (moveDelta - moveMax > 0)
                    {
                        this.dragingMove = 0;
                        moveDelta = moveMax;
                    }
                }
            }
            else if (moveDelta < 0)//向左滑动
            {
                if (isElastic)
                {
                    float lastItemX = this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.x;
                    float lastData = this.showItems[this.showItems.Count - 1].dataIndex;
                    float maxX = viewPortWidth - this.padding_right - itemCellWidth;
                    if (lastItemX < maxX && lastData == this.data.Count - 1)
                    {
                        this.moveScale = 1-(maxX - lastItemX)/ (viewPortWidth - itemCellWidth/2);
                        moveDelta = moveDelta * this.moveScale;

                        this.elasticMove = lastItemX;
                        this.elasticTarget = maxX;
                        this.dragingMove = 0;
                    }
                    else
                    {
                        this.elasticMove = 0;
                        this.moveScale = 0;
                    }
                }
                else
                {
                    //最后一个展示的item需要移动的值
                    float lastShowDelta = this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.x - (viewPortWidth - this.padding_right) + itemCellWidth;
                    //剩下的没有展示的item的移动值
                    float notShowDelta = (this.data.Count - this.showItems[this.showItems.Count - 1].dataIndex - 1) * (itemCellWidth + space);
                    float moveMax = lastShowDelta + notShowDelta;
                    float mOffset = moveMax + moveDelta;//这里moveDelta为负值。
                    if (mOffset < 0)
                    {
                        this.dragingMove = 0;
                        moveDelta = -moveMax;
                    }
                }

            }
        }


        //移动item,并消除已经消失的item
        List<LoopItem> indexArr = new List<LoopItem>();
        for (int i = 0; i < this.showItems.Count; i++)
        {
            float newX = this.showItems[i].rectTransform.anchoredPosition.x + moveDelta;
            this.showItems[i].rectTransform.anchoredPosition = new Vector2(newX, this.showItems[i].rectTransform.anchoredPosition.y);
            if ((moveDelta < 0 && newX < -this.itemCellWidth) || (moveDelta > 0 && newX >= this.viewPortWidth))
            {
                this.RecycleItem(this.showItems[i]);
                indexArr.Add(this.showItems[i]);
            }
        }
        for (int i = 0; i < indexArr.Count; i++)
        {
            this.showItems.Remove(indexArr[i]);
        }



        //新增item。用于补齐
        if (moveDelta > 0)
        {
            //向右移动 计算数组最左边的值
            float nextPosX = this.showItems[0].rectTransform.anchoredPosition.x;
            while (true)
            {
                int nowDataIndex = this.showItems[0].dataIndex - 1;
                if (nowDataIndex < 0)
                {
                    if (this.isItemLoop)
                        nowDataIndex = this.data.Count - 1;
                    else
                        break;
                    
                }

                nextPosX = nextPosX - this.space - this.itemCellWidth;
                if (nextPosX < -this.itemCellWidth)
                    break;

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                if (newItem != null)
                {
                    newItem.rectTransform.anchoredPosition = new Vector2(nextPosX, this.showItems[0].rectTransform.anchoredPosition.y);
                    this.showItems.Insert(0, newItem);
                    this.dataDeal(newItem, this.data, nowDataIndex);
                }
            }
        } 
        else if (moveDelta < 0)
        {
            //向左移动 计算数组右最边的值

            float nextPosX = this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.x;
            while (true)
            {
                int nowDataIndex = this.showItems[this.showItems.Count - 1].dataIndex + 1;
                if (nowDataIndex > this.data.Count - 1)
                {
                    if (this.isItemLoop)
                        nowDataIndex = 0;
                    else
                        break;
                }

                nextPosX = nextPosX + this.space + this.itemCellWidth;
                if (nextPosX > this.viewPortWidth)
                    break;

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                if (newItem != null)
                {
                    newItem.rectTransform.anchoredPosition = new Vector2(nextPosX, this.showItems[0].rectTransform.anchoredPosition.y);
                    this.showItems.Add(newItem);
                    this.dataDeal(newItem, this.data, nowDataIndex);
                }
            }
        }
        this.moveLock = true;
    }

    public void LoopMove_Vertical(float moveDelta)
    {
        if (!isItemLoop)//item不循环的时候，需要限制item是否在data的边界
        {
            if (moveDelta > 0)//向上滑动
            {
                //垂直方向边界值处理
                if (isElastic)
                {
                    float lastItemY = this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.y;
                    float lastData = this.showItems[this.showItems.Count - 1].dataIndex;
                    float bottomY = this.bottomBorder - itemCellHeight;
                    if (lastItemY > bottomY && lastData == this.data.Count - 1)
                    {
                        this.moveScale = 1 - (lastItemY - bottomY) / (viewPortHeight + itemCellHeight / 2);
                        //Debug.LogError("this.moveScale:" + this.moveScale);
                        moveDelta = moveDelta * this.moveScale;

                        this.elasticMove = lastItemY;
                        this.elasticTarget = this.bottomBorder + itemCellHeight;
                        this.dragingMove = 0;
                    }
                    else
                    {
                        this.elasticMove = 0;
                        this.moveScale = 0;
                    }
                }
                else
                {
                    //最后一个展示的item需要移动的值
                    float lastShowDelta = itemCellHeight - this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.y + this.bottomBorder;
                    //剩下的没有展示的item的移动值
                    float notShowDelta = (this.data.Count - this.showItems[this.showItems.Count - 1].dataIndex - 1) * (itemCellHeight + space);
                    float moveMax = lastShowDelta + notShowDelta;
                    float mOffset = moveMax - moveDelta;
                    if (mOffset < 0)
                    {
                        this.dragingMove = 0;
                        moveDelta = moveMax;
                    }
                }
            }
            else if (moveDelta < 0)//向下滑动
            {
                if (isElastic)
                {
                    float lastItemY = this.showItems[0].rectTransform.anchoredPosition.y;
                    float lastData = this.showItems[0].dataIndex;
                    float topY = this.topBorder;
                    if (lastItemY < topY && lastData == 0)
                    {
                        moveScale = 1 - (topY - lastItemY) / (viewPortHeight + itemCellHeight / 2);
                        //Debug.LogError("this.moveScale:" + this.moveScale);
                        moveDelta = moveDelta * moveScale;

                        this.elasticMove = lastItemY;
                        this.elasticTarget = topY;
                        this.dragingMove = 0;
                    }
                    else
                    {
                        this.elasticMove = 0;
                        this.moveScale = 0;
                    }
                }
                else
                {
                    float moveMax = this.showItems[0].rectTransform.anchoredPosition.y - this.topBorder + this.showItems[0].dataIndex * (itemCellHeight + space);
                    if (moveMax + moveDelta < 0)//moveDelta为负值
                    {
                        this.dragingMove = 0;
                        moveDelta = -moveMax;
                    }
                }

            }
        }


        //移动item,并消除已经消失的item
        List<LoopItem> indexArr = new List<LoopItem>();
        for (int i = 0; i < this.showItems.Count; i++)
        {
            float newY = this.showItems[i].rectTransform.anchoredPosition.y + moveDelta;
            this.showItems[i].rectTransform.anchoredPosition = new Vector2(this.showItems[i].rectTransform.anchoredPosition.x, newY);
            if ((moveDelta < 0 && newY < -this.viewPortHeight) || (moveDelta > 0 && newY >= this.itemCellHeight))
            {
                this.RecycleItem(this.showItems[i]);
                indexArr.Add(this.showItems[i]);
            }
        }
        for (int i = 0; i < indexArr.Count; i++)
        {
            this.showItems.Remove(indexArr[i]);
        }



        //新增item。用于补齐
        if (moveDelta > 0)
        {
            //向上滑动
            float nextPosY = this.showItems[this.showItems.Count - 1].rectTransform.anchoredPosition.y;
            while (true)
            {
                int nowDataIndex = this.showItems[this.showItems.Count - 1].dataIndex + 1;
                if (nowDataIndex > this.data.Count - 1)
                {
                    if (this.isItemLoop)
                        nowDataIndex = 0;
                    else
                        break;
                }

                nextPosY = nextPosY - this.space - this.itemCellHeight;
                if (nextPosY < -this.viewPortHeight)
                    break;

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                if (newItem != null)
                {
                    newItem.rectTransform.anchoredPosition = new Vector2(this.showItems[0].rectTransform.anchoredPosition.x, nextPosY);
                    this.showItems.Add(newItem);
                    this.dataDeal(newItem, this.data, nowDataIndex);
                }
            }
        }
        else if (moveDelta < 0)
        {
            //向下滑动
            float nextPosY = this.showItems[0].rectTransform.anchoredPosition.y;
            while (true)
            {
                int nowDataIndex = this.showItems[0].dataIndex - 1;
                if (nowDataIndex < 0)
                {
                    if (this.isItemLoop)
                        nowDataIndex = this.data.Count - 1;
                    else
                        break;

                }

                nextPosY = nextPosY + this.space + this.itemCellHeight;
                if (nextPosY > this.itemCellHeight)
                    break;

                LoopItem newItem = this.GetShowItem(nowDataIndex);
                if (newItem != null)
                {
                    newItem.rectTransform.anchoredPosition = new Vector2(this.showItems[0].rectTransform.anchoredPosition.x,nextPosY);
                    this.showItems.Insert(0, newItem);
                    this.dataDeal(newItem, this.data, nowDataIndex);
                }
            }
        }
        this.moveLock = true;
    }

    /// <summary>
    /// 获取item
    /// </summary>
    /// <param name="dataIndex"></param>
    /// <returns></returns>
    public LoopItem GetShowItem(int dataIndex)
    {
        LoopItem newItem = null;
        int _count = this.data.Count;
        if (dataIndex >= 0 && dataIndex < _count)
        {
            if(_count == 0)
            {
                newItem = this.CreateNewItem(dataIndex);
                this.itemPool.Add(newItem);
            }
            else
            {
                for (int i = 0; i < itemPool.Count; i++)
                {
                    if (!itemPool[i].isWork)
                    {
                        this.itemPool[i].dataIndex = dataIndex;
                        this.itemPool[i].isWork = true;
                        this.itemPool[i].gameObject.SetActive(true);
                        newItem = this.itemPool[i];
                        break;
                    }
                }

                if(newItem == null)
                {
                    newItem = this.CreateNewItem(dataIndex);
                    this.itemPool.Add(newItem);
                }
            }
        }
        if (horizontalType)
        {
            newItem.rectTransform.pivot = new Vector2(0, 0.5f);
            newItem.rectTransform.anchorMin = new Vector2(0, 0.5f);
            newItem.rectTransform.anchorMax = new Vector2(0, 0.5f);
        }
        else if (verticalType)
        {
            newItem.rectTransform.pivot = new Vector2(0.5f, 1);
            newItem.rectTransform.anchorMin = new Vector2(0.5f, 1);
            newItem.rectTransform.anchorMax = new Vector2(0.5f, 1);
        }
        return newItem;
    }

    public LoopItem CreateNewItem(int dataIndex) 
    {
        LoopItem newItem = new LoopItem();
        newItem.gameObject = GameObject.Instantiate(this.origItemTrans, this.rootTrans).gameObject;
        newItem.rectTransform = newItem.gameObject.transform.GetComponent<RectTransform>();
        newItem.dataIndex = dataIndex;
        newItem.isWork = true;
        newItem.gameObject.SetActive(true);

        newItem.name = newItem.gameObject.transform.Find("Text").GetComponent<Text>();
        return newItem;
    }

    /// <summary>
    /// 回收Item
    /// </summary>
    /// <param name="_item"></param>
    public void RecycleItem(LoopItem _item)
    {
        _item.gameObject.SetActive(false);
        _item.isWork = false;
        _item.dataIndex = -1;
    }

    public void LoopMove_Inertia()
    {
        if (!isCanTouchMove)
            return;

        if (this.dragingMove != 0 && this.inertia && elasticMove == 0)
        {
            //在滑动中，但是没有移动的这种行为（触发OnBeginDrag,但是OnDrag没有移动）。
            //插值减小最后一帧move的值（OnDrag中最后一次move不等于0的情况）。
            //这样在放开滑动（触发OnEndDrag）的时候行为更平滑
            if (this.isDraging)
            {
                this.dragingMove = Mathf.Lerp(this.dragingMove, 0, Time.smoothDeltaTime);
            }
            else
            {
                this.dragingMove *= Mathf.Pow(0.135f, Time.smoothDeltaTime);
                if (Mathf.Abs(this.dragingMove) < 0.01)
                    this.dragingMove = 0;
                this.LoopMove(this.dragingMove);
            }
        }

        if (elasticMove != 0 && isElastic && !this.isDraging)
        {
            float newCur = Mathf.SmoothDamp(elasticMove, elasticTarget, ref this.moveSpeed, 0.1f);
            this.LoopMove(newCur - elasticMove);
            Debug.LogError(newCur - elasticMove);
            elasticMove = newCur;
            if (Mathf.Abs(this.moveSpeed) < 0.01f)
            {
                elasticMove = 0;
            }
        }
    }
}
